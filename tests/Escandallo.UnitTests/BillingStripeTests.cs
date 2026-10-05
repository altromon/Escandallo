using Escandallo.Core.Modules.Billing;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-BILLING-STRIPE-001 — Cuota Freemium y Stripe (TSK-006).
/// Requisitos cubiertos: FR-BILLING-001, SEC-REQ-BILLING-001, QR-SEC-001, BR-FREEMIUM-QUOTA-001.
/// </summary>
public sealed class BillingStripeTests
{
    [Fact]
    public void FrBilling001_AllowsUpToTwoFreeEscandallosAndBlocksThirdWithHttp402() {
        // Regla de cuota gratuita (BR-FREEMIUM-QUOTA-001): 2 escandallos activos gratis por centro
        var first = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "none", 0, 0, "crear_escandallo", 1));
        var second = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "none", 1, 0, "crear_escandallo", 1));
        var recalc = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "none", 2, 0, "recalcular_existente", 1));
        var thirdBlocked = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "none", 2, 0, "crear_escandallo", 1));
        Assert.Equal(201, first.HttpStatus);
        Assert.Equal(201, second.HttpStatus);
        Assert.Equal(200, recalc.HttpStatus);
        Assert.Equal(402, thirdBlocked.HttpStatus);
        Assert.Equal("FREEMIUM_QUOTA_EXCEEDED", thirdBlocked.ErrorCode);
        Assert.True(thirdBlocked.UpgradeRequired);
    }

    [Fact]
    public void FrBilling001_BlocksEditingWhenCanceledOverQuotaAndUnlocksAfterArchivingM1() {
        // Mejora M1: desbloqueo mediante archivado de escandallos excedentes sin pérdida de datos
        var lockedEdit = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "past_due", 5, 0, "editar_escandallo", 1));
        var archiveThird = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "canceled", 3, 1, "archivar_escandallo", 1));
        var unlockedEdit = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "canceled", 2, 2, "editar_escandallo", 1));
        Assert.Equal(402, lockedEdit.HttpStatus);
        Assert.Equal(200, archiveThird.HttpStatus);
        Assert.Equal(2, archiveThird.RemainingActiveEscandallos);
        Assert.Equal(200, unlockedEdit.HttpStatus);
    }

    [Fact]
    public void SecReqBilling001_SerializesConcurrentCreationBurstsPreventingRaceBypass() {
        // Mitigación de ABUSE-QUOTA-BYPASS-001: bloqueo pesimista ante 10 peticiones simultáneas
        var burst = BillingQuotaService.EnforceFreemiumQuotaWithLock(new FreemiumQuotaInput(
            "centro-sol", "none", 1, 0, "crear_escandallo", 10));
        Assert.Equal(1, burst.SucceededRequests);
        Assert.Equal(9, burst.RejectedWith402Count);
        Assert.Equal(2, burst.FinalActiveEscandallosCount);
    }

    [Fact]
    public void SecReqBilling001_VerifiesStripeWebhookHmacReplayWindowAndIdempotency() {
        // Verificación criptográfica de Stripe-Signature (HMAC-SHA256) y ventana <= 300s
        var validHook = BillingQuotaService.VerifyAndProcessStripeWebhook(new StripeWebhookInput(
            "evt_valid_001", "customer.subscription.created", "ninguna", 10));
        var forgedHook = BillingQuotaService.VerifyAndProcessStripeWebhook(new StripeWebhookInput(
            "evt_fake_002", "checkout.session.completed", "hmac_invalido", 5));
        var replayHook = BillingQuotaService.VerifyAndProcessStripeWebhook(new StripeWebhookInput(
            "evt_old_003", "invoice.paid", "timestamp_expirado", 600));
        var duplicateHook = BillingQuotaService.VerifyAndProcessStripeWebhook(new StripeWebhookInput(
            "evt_valid_001", "invoice.paid", "event_id_duplicado", 15));
        Assert.Equal(200, validHook.HttpStatus);
        Assert.True(validHook.SubscriptionModified);
        Assert.Equal(400, forgedHook.HttpStatus);
        Assert.False(forgedHook.SubscriptionModified);
        Assert.Equal(400, replayHook.HttpStatus);
        Assert.Equal(200, duplicateHook.HttpStatus);
        Assert.False(duplicateHook.SubscriptionModified);
    }
}
