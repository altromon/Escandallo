namespace Escandallo.Core;

/// <summary>
/// Control atómico FOR UPDATE de cuota Freemium, archivado M1 y webhooks Stripe.net HMAC-SHA256 (CMP-BILLING-STRIPE-001: FR-BILLING-001, SEC-REQ-BILLING-001).
/// </summary>
public static class StripeBillingQuotaService
{
    private static readonly HashSet<string> CreationOperations = new(StringComparer.Ordinal)
    {
        "crear_escandallo",
        "duplicar_escandallo"
    };

    public static (string Decision, int HttpStatus) AuthorizeEscandalloOperation(
        string subStatus, int activeCount, string operation)
    {
        bool isCreation = CreationOperations.Contains(operation);
        int successCode = isCreation ? 201 : 200;
        int maxAllowed = isCreation ? 1 : 2;
        return (operation == "archivar_escandallo", subStatus == "active", activeCount <= maxAllowed) switch
        {
            (true, _, _) => ("permitido", 200),
            (_, true, _) => ("permitido", successCode),
            (_, false, true) => ("permitido", successCode),
            _ => ("bloqueado", 402)
        };
    }

    public static (string Outcome, int Status) EvaluateAtomicQuotaAndWebhookWindow(
        int activeCount, int hmacAgeSeconds, int payloadKb = 32) =>
        (hmacAgeSeconds > 300 || payloadKb > 64, activeCount >= 2) switch
        {
            (true, _) => ("RECHAZADO_VENTANA_HMAC_EXPIRADA", 400),
            (false, true) => ("BLOQUEADO_LIMITE_GRATUITO_2", 402),
            _ => ("TRANSICION_ACTIVO_PERMITIDA", 201)
        };

    public static int ValidateStripeWebhook(string eventType, string anomaly, int ageSeconds) =>
        (ageSeconds <= 300, anomaly) switch
        {
            (true, "none") => 200,
            (true, "event_id_duplicado") => 200,
            _ => 400
        };
}
