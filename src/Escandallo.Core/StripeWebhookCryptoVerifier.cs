using System.Security.Cryptography;
using System.Text;

namespace Escandallo.Core;

/// <summary>
/// Verificador criptográfico real HMAC-SHA256 en tiempo constante y tarifa oficial Stripe (SEC-REQ-BILLING-001, FR-BILLING-001).
/// </summary>
public static class StripeWebhookCryptoVerifier
{
    public static string ComputeSignatureHex(string webhookSecret, long timestampUnix, string rawPayload)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
        byte[] dataBytes = Encoding.UTF8.GetBytes($"{timestampUnix}.{rawPayload}");
        byte[] macBytes = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(macBytes).ToLowerInvariant();
    }

    public static (int HttpStatus, string Verdict) VerifyHmacConstantTime(
        string webhookSecret, long timestampUnix, long nowUnix, string rawPayload, string providedHexSignature)
    {
        string expectedHex = ComputeSignatureHex(webhookSecret, timestampUnix, rawPayload);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedHex);
        byte[] providedBytes = Encoding.UTF8.GetBytes(providedHexSignature.ToLowerInvariant());
        bool validMac = CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        bool withinWindow = Math.Abs(nowUnix - timestampUnix) <= 300;
        return (validMac, withinWindow) switch
        {
            (false, _) => (400, "FIRMA_HMAC_INVALIDA"),
            (true, false) => (400, "TIMESTAMP_FUERA_DE_VENTANA_300S"),
            _ => (200, "WEBHOOK_AUTENTICADO_OK")
        };
    }

    public static (int HttpStatus, decimal EnforcedAmountEur, string Action) ResolveOfficialPlanPrice(
        string requestedPlanId, decimal clientDeclaredAmount) =>
        (requestedPlanId, clientDeclaredAmount) switch
        {
            ("PLAN_MENSUAL_EUR", 14.90m) => (200, 14.90m, "CREAR_SESION_STRIPE_14_90_EUR"),
            ("PLAN_MENSUAL_EUR", _) => (200, 14.90m, "IGNORAR_IMPORTE_CLIENTE_USAR_TARIFA_14_90"),
            ("PLAN_ANUAL_EUR", _) => (200, 149.00m, "CREAR_SESION_STRIPE_149_00_EUR"),
            _ => (400, 0.00m, "RECHAZAR_PLAN_NO_RECONOCIDO")
        };
}
