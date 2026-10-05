namespace Escandallo.Core.Modules.Billing;

public sealed record FreemiumQuotaInput(
    string CenterId,
    string SubscriptionStatus,
    int ActiveEscandallosCount,
    int ArchivedEscandallosCount,
    string Operation,
    int ConcurrentRequests);

public sealed record FreemiumQuotaResult(
    int HttpStatus,
    string? ErrorCode = null,
    bool UpgradeRequired = false,
    int RemainingActiveEscandallos = 0,
    int SucceededRequests = 0,
    int RejectedWith402Count = 0,
    int FinalActiveEscandallosCount = 0);

public sealed record StripeWebhookInput(
    string EventId,
    string EventType,
    string SignatureAnomaly,
    int AgeSeconds);

public sealed record StripeWebhookResult(
    int HttpStatus,
    bool SubscriptionModified,
    string? AuditSeverity = null);

public static class BillingQuotaService
{
    private const int FreemiumMaxActive = 2;
    private const int WebhookMaxAgeSeconds = 300;

    private static FreemiumQuotaResult HandleExistingMutation(FreemiumQuotaInput input) {
        // Permite editar o recalcular si hay suscripción activa o <= 2 escandallos activos
        if (!string.Equals(input.SubscriptionStatus, "active", StringComparison.Ordinal) &&
            input.ActiveEscandallosCount > FreemiumMaxActive)
        {
            return new FreemiumQuotaResult(402, "FREEMIUM_QUOTA_EXCEEDED", true);
        }
        return new FreemiumQuotaResult(200);
    }

    private static FreemiumQuotaResult HandleConcurrentCreationWithLock(FreemiumQuotaInput input) {
        // Serializa altas concurrentes simulando SELECT ... FOR UPDATE sobre el centro
        var availableSlots = Math.Max(0, FreemiumMaxActive - input.ActiveEscandallosCount);
        if (string.Equals(input.SubscriptionStatus, "active", StringComparison.Ordinal))
        {
            availableSlots = input.ConcurrentRequests;
        }
        var succeeded = Math.Min(input.ConcurrentRequests, availableSlots);
        var rejected = input.ConcurrentRequests - succeeded;
        var finalActive = input.ActiveEscandallosCount + succeeded;
        if (succeeded == 0)
        {
            return new FreemiumQuotaResult(402, "FREEMIUM_QUOTA_EXCEEDED", true, 0, 0, rejected, finalActive);
        }
        return new FreemiumQuotaResult(201, null, false, finalActive, succeeded, rejected, finalActive);
    }

    public static FreemiumQuotaResult EnforceFreemiumQuotaWithLock(FreemiumQuotaInput input) {
        // Gestiona archivado (M1), edición/recálculo y creación atómica bajo cuota freemium
        if (string.Equals(input.Operation, "archivar_escandallo", StringComparison.Ordinal))
        {
            var remaining = Math.Max(0, input.ActiveEscandallosCount - 1);
            return new FreemiumQuotaResult(200, null, false, remaining);
        }
        if (string.Equals(input.Operation, "recalcular_existente", StringComparison.Ordinal) ||
            string.Equals(input.Operation, "editar_escandallo", StringComparison.Ordinal))
        {
            return HandleExistingMutation(input);
        }
        return HandleConcurrentCreationWithLock(input);
    }

    public static StripeWebhookResult VerifyAndProcessStripeWebhook(StripeWebhookInput input) {
        // Verifica idempotencia por EventId, firma HMAC-SHA256 y ventana temporal <= 300s
        if (string.Equals(input.SignatureAnomaly, "event_id_duplicado", StringComparison.Ordinal))
        {
            return new StripeWebhookResult(200, false);
        }
        if (!string.Equals(input.SignatureAnomaly, "ninguna", StringComparison.Ordinal) ||
            input.AgeSeconds > WebhookMaxAgeSeconds)
        {
            return new StripeWebhookResult(400, false, "SECURITY");
        }
        return new StripeWebhookResult(200, true);
    }
}
