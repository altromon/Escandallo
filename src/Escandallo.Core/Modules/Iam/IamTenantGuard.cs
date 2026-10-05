using System.Text.RegularExpressions;

namespace Escandallo.Core.Modules.Iam;

public sealed record SecurityAuditEvent(string Severity, string Reason);

public sealed record CrossCenterCopyInput(
    string AuthenticatedUserEmail,
    string SourceCenterId,
    string TargetCenterId,
    IReadOnlyDictionary<string, string> OwnerByCenter,
    int CatalogItemsCount);

public sealed record CrossCenterCopyResult(
    int HttpStatus,
    int CopiedItems,
    IReadOnlyList<string> IsolatedForeignCenters,
    SecurityAuditEvent? AuditEvent = null);

public sealed record SessionRlsInput(
    string AuthenticatedUserId,
    string ActiveCenterId,
    string TargetCenterId,
    string Operation);

public sealed record SessionRlsResult(
    int HttpStatus,
    object? LeakedTenantData = null,
    string? AuditSeverity = null);

public sealed record TenantRateLimitInput(
    string TenantId,
    string Operation,
    int RequestsInWindow,
    int MaxAllowedPerMinute);

public sealed record TenantRateLimitResult(
    int HttpStatus,
    int RetryAfterSeconds,
    string LogSeverity);

public static class IamTenantGuard
{
    private static readonly Regex UuidRegex = new(
        @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<string> ListForeignCenters(IReadOnlyDictionary<string, string> ownerByCenter, string email) {
        // Filtra todos los centros cuyo propietario no coincide con el usuario autenticado
        return ownerByCenter
            .Where(kvp => !string.Equals(kvp.Value, email, StringComparison.Ordinal))
            .Select(kvp => kvp.Key)
            .ToArray();
    }

    public static bool IsCenterOwnedBy(IReadOnlyDictionary<string, string> ownerByCenter, string centerId, string email) {
        // Comprueba la titularidad exacta del centro solicitado en el mapa de sesión
        return ownerByCenter.TryGetValue(centerId, out var owner) &&
               string.Equals(owner, email, StringComparison.Ordinal);
    }

    public static CrossCenterCopyResult AuthorizeCrossCenterCopy(CrossCenterCopyInput input) {
        // Valida parámetros de entrada, propiedad de ambos centros y límite de catálogo (M4)
        var foreign = ListForeignCenters(input.OwnerByCenter, input.AuthenticatedUserEmail);
        if (string.IsNullOrWhiteSpace(input.SourceCenterId) || string.IsNullOrWhiteSpace(input.TargetCenterId))
        {
            return new CrossCenterCopyResult(400, 0, foreign);
        }
        var ownsBoth = IsCenterOwnedBy(input.OwnerByCenter, input.SourceCenterId, input.AuthenticatedUserEmail) &&
                       IsCenterOwnedBy(input.OwnerByCenter, input.TargetCenterId, input.AuthenticatedUserEmail);
        if (!ownsBoth)
        {
            var audit = new SecurityAuditEvent("SECURITY", "CROSS_TENANT_IDOR_ATTEMPT");
            return new CrossCenterCopyResult(403, 0, foreign, audit);
        }
        if (input.CatalogItemsCount < 0 || input.CatalogItemsCount > 5000)
        {
            return new CrossCenterCopyResult(422, 0, foreign);
        }
        return new CrossCenterCopyResult(200, input.CatalogItemsCount, foreign);
    }

    public static SessionRlsResult VerifySessionAndRls(SessionRlsInput input) {
        // Verifica formato UUID v4 y coincidencia entre sesión activa y centro objetivo (RLS)
        if (!UuidRegex.IsMatch(input.TargetCenterId) || !UuidRegex.IsMatch(input.ActiveCenterId))
        {
            return new SessionRlsResult(400);
        }
        if (!string.Equals(input.ActiveCenterId, input.TargetCenterId, StringComparison.OrdinalIgnoreCase))
        {
            var status = 403;
            if (input.Operation.StartsWith("PATCH", StringComparison.OrdinalIgnoreCase))
            {
                status = 404;
            }
            return new SessionRlsResult(status, null, "SECURITY");
        }
        return new SessionRlsResult(200);
    }

    public static TenantRateLimitResult CheckTenantRateLimit(TenantRateLimitInput input) {
        // Aplica cuota de peticiones por minuto por inquilino y emite cabecera Retry-After
        if (input.RequestsInWindow > input.MaxAllowedPerMinute)
        {
            var severity = "SECURITY";
            if (string.Equals(input.Operation, "actualizacion_coste_con_cascada", StringComparison.Ordinal))
            {
                severity = "WARN";
            }
            return new TenantRateLimitResult(429, 60, severity);
        }
        return new TenantRateLimitResult(200, 0, "INFO");
    }
}
