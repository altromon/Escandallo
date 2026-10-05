using Escandallo.Core.Modules.Iam;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-IAM-TENANT-001 (TSK-001).
/// Requisitos cubiertos: FR-TENANT-001, SEC-REQ-TENANT-001, SEC-REQ-DOS-001, QR-SEC-001.
/// </summary>
public sealed class IamTenantRlsTests
{
    private static readonly IReadOnlyDictionary<string, string> DefaultOwners = new Dictionary<string, string>
    {
        ["Centro Sol"] = "ana@estetica.es",
        ["Centro Luna"] = "ana@estetica.es",
        ["Centro Rival"] = "rival@otro.es"
    };

    [Fact]
    public void FrTenant001_AllowsCatalogCopyBetweenOwnCentersAndIsolatesForeignCenters() {
        // Escenario nominal: copia de catálogo entre dos centros de la misma propietaria (M4)
        var result = IamTenantGuard.AuthorizeCrossCenterCopy(new CrossCenterCopyInput(
            "ana@estetica.es", "Centro Sol", "Centro Luna", DefaultOwners, 45));
        Assert.Equal(200, result.HttpStatus);
        Assert.Equal(45, result.CopiedItems);
        Assert.Contains("Centro Rival", result.IsolatedForeignCenters);
    }

    [Fact]
    public void FrTenant001_ValidatesCatalogCopyBoundariesZeroOneAndFiveThousand() {
        // Escenario @boundary: fronteras exactas de 0, 1 y 5000 ítems de catálogo
        var emptyCopy = IamTenantGuard.AuthorizeCrossCenterCopy(new CrossCenterCopyInput(
            "ana@estetica.es", "Centro Sol", "Centro Luna", DefaultOwners, 0));
        var maxCopy = IamTenantGuard.AuthorizeCrossCenterCopy(new CrossCenterCopyInput(
            "ana@estetica.es", "Centro Sol", "Centro Luna", DefaultOwners, 5000));
        var overMaxCopy = IamTenantGuard.AuthorizeCrossCenterCopy(new CrossCenterCopyInput(
            "ana@estetica.es", "Centro Sol", "Centro Luna", DefaultOwners, 5001));
        Assert.Equal(200, emptyCopy.HttpStatus);
        Assert.Equal(200, maxCopy.HttpStatus);
        Assert.Equal(422, overMaxCopy.HttpStatus);
    }

    [Fact]
    public void FrTenant001_BlocksCrossTenantCopyAndGeneratesSecurityAuditEvent() {
        // Escenario @invalid: bloqueo de exfiltración cross-tenant y parámetros malformados
        var forbidden = IamTenantGuard.AuthorizeCrossCenterCopy(new CrossCenterCopyInput(
            "ana@estetica.es", "Centro Rival", "Centro Sol", DefaultOwners, 10));
        var malformed = IamTenantGuard.AuthorizeCrossCenterCopy(new CrossCenterCopyInput(
            "ana@estetica.es", "", "Centro Sol", DefaultOwners, 10));
        Assert.Equal(403, forbidden.HttpStatus);
        Assert.Equal("SECURITY", forbidden.AuditEvent?.Severity);
        Assert.Equal("CROSS_TENANT_IDOR_ATTEMPT", forbidden.AuditEvent?.Reason);
        Assert.Equal(400, malformed.HttpStatus);
    }

    [Fact]
    public void SecReqTenant001_EnforcesZeroTrustRlsAndUuidValidation() {
        // Mitigación de ABUSE-IDOR-TENANT-001: validación UUID v4 y aislamiento RLS por centro
        var ownAccess = IamTenantGuard.VerifySessionAndRls(new SessionRlsInput(
            "user-a", "00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000001", "GET /api/v1/centers/c1/escandallos"));
        var crossTenant = IamTenantGuard.VerifySessionAndRls(new SessionRlsInput(
            "user-a", "00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002", "PATCH /api/v1/centers/c2/costs"));
        var malformedUuid = IamTenantGuard.VerifySessionAndRls(new SessionRlsInput(
            "user-a", "00000000-0000-0000-0000-000000000001", "1 OR 1=1", "GET /api/v1/centers/bad/costs"));
        Assert.Equal(200, ownAccess.HttpStatus);
        Assert.Equal(404, crossTenant.HttpStatus);
        Assert.Null(crossTenant.LeakedTenantData);
        Assert.Equal(400, malformedUuid.HttpStatus);
    }

    [Fact]
    public void SecReqDos001_AppliesPerTenantRateLimitingWithRetryAfterHeader() {
        // Mitigación de ABUSE-DOS-CASCADE-001 y QR-SEC-001: límite de 30 mutaciones/minuto
        var allowed = IamTenantGuard.CheckTenantRateLimit(new TenantRateLimitInput(
            "tenant-1", "actualizacion_coste_con_cascada", 30, 30));
        var throttled = IamTenantGuard.CheckTenantRateLimit(new TenantRateLimitInput(
            "tenant-1", "actualizacion_coste_con_cascada", 31, 30));
        Assert.Equal(200, allowed.HttpStatus);
        Assert.Equal(429, throttled.HttpStatus);
        Assert.Equal(60, throttled.RetryAfterSeconds);
        Assert.Equal("WARN", throttled.LogSeverity);
    }
}
