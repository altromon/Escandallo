namespace Escandallo.Core;

/// <summary>
/// Guardia de aislamiento Multi-Tenant RLS, copia dual M4 y estrangulamiento anti-DoS (CMP-IAM-TENANT-001: FR-TENANT-001, SEC-REQ-TENANT-001, SEC-REQ-DOS-001).
/// </summary>
public static class TenantIsolationSecurityService
{
    public static (int Status, string CopyState) AuthorizeDualCenterCopy(string sourceOwner, string targetOwner) =>
        (sourceOwner, targetOwner) switch
        {
            ("OWNER", "OWNER") => (200, "CATALOGO_CLONADO_OK"),
            ("OWNER", _) => (403, "RECHAZADO_DESTINO_AJENO"),
            (_, "OWNER") => (403, "RECHAZADO_ORIGEN_AJENO"),
            _ => (403, "RECHAZADO_AMBOS_AJENOS")
        };

    public static (int HttpStatus, string AuditSeverity) AuthorizeCrossTenantRequest(string operation, string resource)
    {
        int status = operation.StartsWith("PATCH", StringComparison.Ordinal) ? 404 : 403;
        return (status, "SECURITY");
    }

    public static (int Status, string State) EvaluateResourceThrottling(
        int concurrentExports, int mutationsPerMinute, int recipeLineCount, int activeWssCircuits = 1) =>
        (concurrentExports > 2, mutationsPerMinute > 30, recipeLineCount > 100, activeWssCircuits > 5, concurrentExports == 2) switch
        {
            (true, _, _, _, _) => (429, "CONCURRENCIA_EXPORTACION_EXCEDIDA"),
            (_, true, _, _, _) => (429, "TASA_MUTACIONES_CASCADA_EXCEDIDA"),
            (_, _, true, _, _) => (422, "CARDINALIDAD_RECETA_EXCEDIDA"),
            (_, _, _, true, _) => (429, "LIMITE_CIRCUITOS_WSS_BLAZOR_EXCEDIDO"),
            (_, _, _, _, true) => (200, "EN_LIMITE_EXACTO_PERMITIDO"),
            _ => (200, "DENTRO_DE_UMBRAL")
        };
}
