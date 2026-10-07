namespace Escandallo.Core;

/// <summary>
/// Constructor seguro de sesión PostgreSQL RLS validando estrictamente UUID contra inyección SQL (SEC-REQ-TENANT-001).
/// </summary>
public static class RlsSessionCommandBuilder
{
    public static (int HttpStatus, string SqlOrError) BuildSetLocalCenterCommand(string rawCenterId)
    {
        bool isValidGuid = Guid.TryParse(rawCenterId, out Guid centerGuid) && centerGuid != Guid.Empty;
        return isValidGuid
            ? (200, $"SET LOCAL app.current_center_id = '{centerGuid:D}';")
            : (403, "RECHAZADO_CONTEXTO_RLS_INVALIDO_O_SQLI");
    }
}
