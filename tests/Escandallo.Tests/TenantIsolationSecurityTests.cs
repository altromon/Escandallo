using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas unitarias y BDD en estado ROJO (FAILING) para FR-TENANT-001, SEC-REQ-TENANT-001 y SEC-REQ-DOS-001.
/// Verifica el aislamiento multi-tenant RLS, la copia dual de catálogo M4 y el estrangulamiento anti-DoS.
/// </summary>
public sealed class TenantIsolationSecurityTests
{
    /// <summary>
    /// Escenario Nominal y Frontera (FR-TENANT-001 / SEC-REQ-TENANT-001): Validación dual de titularidad en copia de catálogo M4.
    /// </summary>
    [Theory]
    [InlineData("OWNER", "OWNER", 200, "CATALOGO_CLONADO_OK")]
    [InlineData("OWNER", "NONE", 403, "RECHAZADO_DESTINO_AJENO")]
    [InlineData("NONE", "OWNER", 403, "RECHAZADO_ORIGEN_AJENO")]
    [InlineData("NONE", "NONE", 403, "RECHAZADO_AMBOS_AJENOS")]
    [Trait("Requirement", "FR-TENANT-001")]
    [Trait("Requirement", "SEC-REQ-TENANT-001")]
    [Trait("BusinessRule", "BR-TENANT-ISOLATION-001")]
    public void CopyCatalogBetweenCenters_BoundaryDualOwnership_EnforcesStrictTenantIsolation(
        string sourceOwnership,
        string targetOwnership,
        int expectedHttpStatus,
        string expectedCopyState)
    {
        // Act: Estado ROJO hasta implementar CMP-IAM-TENANT-001
        (int status, string copyState) = InvokeUnimplementedDualCenterCopyGuard(sourceOwnership, targetOwnership);

        // Assert
        Assert.Equal(expectedHttpStatus, status);
        Assert.Equal(expectedCopyState, copyState);
    }

    /// <summary>
    /// Escenarios de Ataque IDOR / Fuera de Rango (SEC-REQ-TENANT-001): Bloqueo de acceso cross-tenant y emisión de alerta SECURITY.
    /// </summary>
    [Theory]
    [InlineData("GET /costs", "center_id=C_AJENO", 403, "SECURITY")]
    [InlineData("PATCH /costs/cost-uuid-ajeno", "cost_id=cost-uuid-ajeno", 404, "SECURITY")]
    [InlineData("POST /reports/export-pdf", "center_id=C_AJENO", 403, "SECURITY")]
    [InlineData("POST /reports/export-xlsx", "center_id=C_AJENO", 403, "SECURITY")]
    [InlineData("WSS /_blazor InvokeSaveEscandallo", "parameter_center=C_AJENO", 403, "SECURITY")]
    [Trait("Requirement", "SEC-REQ-TENANT-001")]
    public void AuthorizeCrossTenantRequest_IdorAttackVectors_RejectsAndEmitsSecurityAudit(
        string endpointOperation,
        string foreignResourceRef,
        int expectedHttpStatus,
        string expectedAuditSeverity)
    {
        // Act: Estado ROJO
        (int httpStatus, string auditSeverity) = InvokeUnimplementedIdorProtection(endpointOperation, foreignResourceRef);

        // Assert
        Assert.Equal(expectedHttpStatus, httpStatus);
        Assert.Equal(expectedAuditSeverity, auditSeverity);
    }

    /// <summary>
    /// Análisis de Frontera Exacta Anti-DoS (SEC-REQ-DOS-001): Concurrencia de informes (2), mutaciones (30/min), líneas (100) y circuitos WSS (5).
    /// </summary>
    [Theory]
    [InlineData(1, 29, 99, 4, 200, "DENTRO_DE_UMBRAL")]
    [InlineData(2, 30, 100, 5, 200, "EN_LIMITE_EXACTO_PERMITIDO")]
    [InlineData(3, 10, 50, 5, 429, "CONCURRENCIA_EXPORTACION_EXCEDIDA")]
    [InlineData(1, 31, 50, 5, 429, "TASA_MUTACIONES_CASCADA_EXCEDIDA")]
    [InlineData(1, 10, 101, 5, 422, "CARDINALIDAD_RECETA_EXCEDIDA")]
    [InlineData(2, 30, 100, 6, 429, "LIMITE_CIRCUITOS_WSS_BLAZOR_EXCEDIDO")]
    [Trait("Requirement", "SEC-REQ-DOS-001")]
    public void EvaluateResourceThrottling_ExactBoundaries_ReturnsExpectedHttpStatusAndState(
        int concurrentExports,
        int mutationsPerMinute,
        int recipeLineCount,
        int activeWssCircuits,
        int expectedHttpStatus,
        string expectedThrottlingState)
    {
        // Act: Estado ROJO
        (int status, string state) = InvokeUnimplementedDosThrottler(
            concurrentExports, mutationsPerMinute, recipeLineCount, activeWssCircuits);

        // Assert
        Assert.Equal(expectedHttpStatus, status);
        Assert.Equal(expectedThrottlingState, state);
    }

    [Theory]
    [InlineData("cliente-a@estetica.es", "login_credenciales_validas", 200, "AUTENTICADO_CLIENTE_A")]
    [InlineData("cliente-b@estetica.es", "login_credenciales_validas", 200, "AUTENTICADO_CLIENTE_B")]
    [InlineData("cliente-a@estetica.es", "logout_sesion_activa", 200, "SESION_CERRADA")]
    [InlineData("cliente-a@estetica.es", "login_password_incorrecto", 401, "RECHAZADO_CREDENCIALES")]
    [InlineData("atacante@estetica.es", "registro_con_rol_maintainer", 403, "BLOQUEADO_OVERPOSTING")]
    [InlineData("nuevo@estetica.es", "registro_cuenta_valida", 201, "CUENTA_CREADA")]
    [Trait("Requirement", "FR-AUTH-001")]
    [Trait("Requirement", "FR-TENANT-001")]
    public void AuthenticateAndRegisterAccount_EnforcesCredentialsAndAntiOverposting(string email, string op, int expectedStatus, string expectedState) {
        Escandallo.Core.MultiTenantAppStore.ResetToSeed();
        (int status, string state) = InvokeAuthScenario(email, op);
        Assert.Equal(expectedStatus, status);
        Assert.Equal(expectedState, state);
    }

    private static (int Status, string State) InvokeAuthScenario(string email, string op) {
        if (op == "login_credenciales_validas") { var r = Escandallo.Core.TenantAuthService.Login(email, "Estetica2026!"); return (r.HttpStatus, r.SessionState); }
        if (op == "logout_sesion_activa") { var login = Escandallo.Core.TenantAuthService.Login(email, "Estetica2026!"); return Escandallo.Core.TenantAuthService.Logout(login.Token); }
        if (op == "login_password_incorrecto") { var r = Escandallo.Core.TenantAuthService.Login(email, "WrongPass"); return (r.HttpStatus, r.SessionState); }
        var reg = Escandallo.Core.TenantAuthService.Register(new(email, "NuevaClave2026!", "Titular", "Estética Serrano", op == "registro_con_rol_maintainer" ? "maintainer" : null));
        return (reg.HttpStatus, reg.SessionState);
    }

    private static (int Status, string CopyState) InvokeUnimplementedDualCenterCopyGuard(string sourceOwner, string targetOwner) { return Escandallo.Core.TenantIsolationSecurityService.AuthorizeDualCenterCopy(sourceOwner, targetOwner); }

    private static (int HttpStatus, string AuditSeverity) InvokeUnimplementedIdorProtection(string operation, string resource) { return Escandallo.Core.TenantIsolationSecurityService.AuthorizeCrossTenantRequest(operation, resource); }

    private static (int Status, string State) InvokeUnimplementedDosThrottler(int exports, int mutations, int lines, int wssCircuits) { return Escandallo.Core.TenantIsolationSecurityService.EvaluateResourceThrottling(exports, mutations, lines, wssCircuits); }
}
