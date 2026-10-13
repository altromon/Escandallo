using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas unitarias y BDD en estado ROJO (FAILING) para FR-BILLING-001, SEC-REQ-BILLING-001 y BR-FREEMIUM-QUOTA-001.
/// Verifica la cuota gratuita de 2 escandallos activos, el bloqueo/desbloqueo por archivado M1 y la firma HMAC de Stripe.
/// </summary>
public sealed class StripeBillingQuotaTests
{
    /// <summary>
    /// Escenario Nominal y Análisis de Frontera (FR-BILLING-001): Operaciones permitidas y bloqueadas según suscripción y activos.
    /// </summary>
    [Theory]
    [InlineData("none", 1, "crear_escandallo", "permitido", 201)]
    [InlineData("none", 2, "recalcular_existente", "permitido", 200)]
    [InlineData("none", 2, "exportar_escandallo", "permitido", 200)]
    [InlineData("none", 2, "duplicar_escandallo", "bloqueado", 402)]
    [InlineData("canceled", 4, "editar_escandallo", "bloqueado", 402)]
    [InlineData("canceled", 4, "archivar_escandallo", "permitido", 200)]
    [InlineData("canceled", 2, "editar_escandallo", "permitido", 200)]
    [InlineData("active", 15, "crear_escandallo", "permitido", 201)]
    [Trait("Requirement", "FR-BILLING-001")]
    [Trait("BusinessRule", "BR-FREEMIUM-QUOTA-001")]
    public void AuthorizeEscandalloOperation_BoundarySubscriptionAndActiveCount_ReturnsExpectedDecision(
        string subscriptionStatus,
        int activeEscandallosCount,
        string operationName,
        string expectedDecision,
        int expectedHttpStatus)
    {
        // Act: Estado ROJO hasta implementar CMP-BILLING-STRIPE-001
        (string decision, int httpStatus) = InvokeUnimplementedQuotaGuard(
            subscriptionStatus, activeEscandallosCount, operationName);

        // Assert
        Assert.Equal(expectedDecision, decision);
        Assert.Equal(expectedHttpStatus, httpStatus);
    }

    /// <summary>
    /// Análisis de Frontera y Concurrencia (SEC-REQ-BILLING-001): Bloqueo pesimista FOR UPDATE y ventana HMAC de 300s.
    /// </summary>
    [Theory]
    [InlineData(0, 299, "TRANSICION_ACTIVO_PERMITIDA", 201)]
    [InlineData(1, 300, "TRANSICION_ACTIVO_PERMITIDA", 201)]
    [InlineData(2, 100, "BLOQUEADO_LIMITE_GRATUITO_2", 402)]
    [InlineData(1, 301, "RECHAZADO_VENTANA_HMAC_EXPIRADA", 400)]
    [Trait("Requirement", "SEC-REQ-BILLING-001")]
    public void EvaluateAtomicQuotaAndWebhookWindow_BoundaryValues_ReturnsExpectedGuardResult(
        int currentActiveCount,
        int hmacAgeSeconds,
        string expectedGuardOutcome,
        int expectedHttpStatus)
    {
        // Act: Estado ROJO
        (string outcome, int status) = InvokeUnimplementedAtomicBillingCheck(currentActiveCount, hmacAgeSeconds);

        // Assert
        Assert.Equal(expectedGuardOutcome, outcome);
        Assert.Equal(expectedHttpStatus, status);
    }

    /// <summary>
    /// Escenarios Fuera de Rango / Ataques (SEC-REQ-BILLING-001): Spoofing de firma Stripe, replay e idempotencia.
    /// </summary>
    [Theory]
    [InlineData("checkout.session.completed", "firma_hmac_invalida", 10, 400)]
    [InlineData("invoice.paid", "cabecera_ausente", 5, 400)]
    [InlineData("checkout.session.completed", "timestamp_expirado_replay", 600, 400)]
    [InlineData("checkout.session.completed", "payload_raw_modificado_en_stream", 12, 400)]
    [InlineData("customer.subscription.updated", "event_id_duplicado", 15, 200)]
    [Trait("Requirement", "SEC-REQ-BILLING-001")]
    public void ProcessStripeWebhook_ForgedOrReplayedPayloads_RejectsInvalidAndHandlesIdempotence(
        string eventType,
        string signatureAnomaly,
        int ageSeconds,
        int expectedHttpStatus)
    {
        // Act: Estado ROJO
        int actualStatus = InvokeUnimplementedStripeWebhookValidator(eventType, signatureAnomaly, ageSeconds);

        // Assert
        Assert.Equal(expectedHttpStatus, actualStatus);
    }

    private static (string Decision, int HttpStatus) InvokeUnimplementedQuotaGuard(string subStatus, int activeCount, string operation) { return Escandallo.Core.StripeBillingQuotaService.AuthorizeEscandalloOperation(subStatus, activeCount, operation); }

    private static (string Outcome, int Status) InvokeUnimplementedAtomicBillingCheck(int activeCount, int hmacAgeSeconds) { return Escandallo.Core.StripeBillingQuotaService.EvaluateAtomicQuotaAndWebhookWindow(activeCount, hmacAgeSeconds); }

    private static int InvokeUnimplementedStripeWebhookValidator(string eventType, string anomaly, int ageSeconds) { return Escandallo.Core.StripeBillingQuotaService.ValidateStripeWebhook(eventType, anomaly, ageSeconds); }
}
