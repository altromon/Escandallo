using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas criptográficas y de endurecimiento de bajo nivel en .NET para SEC-REQ-BILLING-001,
/// SEC-REQ-XLSX-001, SEC-REQ-OBS-001, SEC-REQ-TENANT-001 y QR-SEC-001.
/// </summary>
public sealed class SecurityHardeningCryptoTests
{
    /// <summary>
    /// Verificación criptográfica real HMAC-SHA256 en tiempo constante (SEC-REQ-BILLING-001): FixedTimeEquals y ventana de 300s.
    /// </summary>
    [Fact]
    [Trait("Requirement", "SEC-REQ-BILLING-001")]
    [Trait("Requirement", "QR-SEC-001")]
    public void VerifyHmacConstantTime_ValidExpiredAndTamperedSignatures_EnforcesCryptographicIntegrity()
    {
        string secret = "whsec_escandallo_test_key_2026";
        long timestamp = 1760000000L;
        string payload = "{\"id\":\"evt_01\",\"type\":\"checkout.session.completed\"}";
        string validHex = Escandallo.Core.StripeWebhookCryptoVerifier.ComputeSignatureHex(secret, timestamp, payload);

        (int okStatus, string okVerdict) = InvokeHmacCheck(secret, timestamp, timestamp + 120, payload, validHex);
        (int expiredStatus, string expiredVerdict) = InvokeHmacCheck(secret, timestamp, timestamp + 301, payload, validHex);
        (int forgedStatus, string forgedVerdict) = InvokeHmacCheck(secret, timestamp, timestamp + 10, payload, "0000deadbeef");

        Assert.Equal((200, "WEBHOOK_AUTENTICADO_OK"), (okStatus, okVerdict));
        Assert.Equal((400, "TIMESTAMP_FUERA_DE_VENTANA_300S"), (expiredStatus, expiredVerdict));
        Assert.Equal((400, "FIRMA_HMAC_INVALIDA"), (forgedStatus, forgedVerdict));
    }

    /// <summary>
    /// Imposición de tarifas oficiales en backend (FR-BILLING-001 / SEC-REQ-BILLING-001): Ignora importes manipulados por el cliente.
    /// </summary>
    [Theory]
    [InlineData("PLAN_MENSUAL_EUR", 14.90, 200, 14.90, "CREAR_SESION_STRIPE_14_90_EUR")]
    [InlineData("PLAN_MENSUAL_EUR", 0.01, 200, 14.90, "IGNORAR_IMPORTE_CLIENTE_USAR_TARIFA_14_90")]
    [InlineData("PLAN_ANUAL_EUR", 149.00, 200, 149.00, "CREAR_SESION_STRIPE_149_00_EUR")]
    [InlineData("PLAN_GRATIS_HACK", 0.00, 400, 0.00, "RECHAZAR_PLAN_NO_RECONOCIDO")]
    [Trait("Requirement", "FR-BILLING-001")]
    [Trait("Requirement", "SEC-REQ-BILLING-001")]
    public void ResolveOfficialPlanPrice_ClientTamperedAmounts_EnforcesBackendTariff(
        string planId, decimal clientAmount, int expectedStatus, decimal expectedEnforcedAmount, string expectedAction)
    {
        (int status, decimal enforcedAmount, string action) = InvokePlanPriceResolver(planId, clientAmount);

        Assert.Equal(expectedStatus, status);
        Assert.Equal(expectedEnforcedAmount, enforcedAmount);
        Assert.Equal(expectedAction, action);
    }

    /// <summary>
    /// Parseo XML real con DtdProcessing.Prohibit (SEC-REQ-XLSX-001): Bloqueo de payload XXE real y escape físico de fórmulas DDE.
    /// </summary>
    [Theory]
    [InlineData("<worksheet><row><c>34.51</c></row></worksheet>", "=CMD|'/C calc'!A0", 200, "XML_SEGURO_SIN_DTD", "'=CMD|'/C calc'!A0")]
    [InlineData("<!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><worksheet>&xxe;</worksheet>", "+SUM(A1)", 422, "RECHAZADO_DTD_XXE", "'+SUM(A1)")]
    [Trait("Requirement", "SEC-REQ-XLSX-001")]
    public void InspectXmlPayloadAndEscapeDde_RealXxeAndFormulaPayloads_BlocksDtdAndPrefixesApostrophe(
        string xmlPayload, string rawCell, int expectedStatus, string expectedVerdict, string expectedEscapedCell)
    {
        (int status, string verdict) = InvokeXmlInspector(xmlPayload);
        string escapedCell = InvokeDdeEscaper(rawCell);

        Assert.Equal(expectedStatus, status);
        Assert.Equal(expectedVerdict, verdict);
        Assert.Equal(expectedEscapedCell, escapedCell);
    }

    /// <summary>
    /// Sanitización real Anti-Log-Injection CRLF y bloqueo de SQL Injection en SET LOCAL RLS (SEC-REQ-OBS-001, SEC-REQ-TENANT-001).
    /// </summary>
    [Theory]
    [InlineData("user_input\r\n[INFO] FakeAdminLogin", "d290f1ee-6c54-4b01-90e6-d701748f0851", "user_input\\r\\n[INFO] FakeAdminLogin", 200)]
    [InlineData("normal_log", "00000000-0000-0000-0000-000000000000", "normal_log", 403)]
    [InlineData("attack_log", "11111111-1111-1111-1111-111111111111'; DROP TABLE centers;--", "attack_log", 403)]
    [Trait("Requirement", "SEC-REQ-OBS-001")]
    [Trait("Requirement", "SEC-REQ-TENANT-001")]
    public void SanitizeCrlfAndBuildRlsSession_LogForgeryAndSqliPayloads_NeutralizesCrlfAndRejectsInvalidGuids(
        string rawLogInput, string rawCenterId, string expectedSanitizedLog, int expectedRlsStatus)
    {
        string sanitizedLog = InvokeLogSanitizer("user_note", rawLogInput);
        (int rlsStatus, _) = InvokeRlsBuilder(rawCenterId);

        Assert.Equal(expectedSanitizedLog, sanitizedLog);
        Assert.Equal(expectedRlsStatus, rlsStatus);
    }

    private static (int Status, string Verdict) InvokeHmacCheck(string secret, long ts, long now, string payload, string sig) { return Escandallo.Core.StripeWebhookCryptoVerifier.VerifyHmacConstantTime(secret, ts, now, payload, sig); }

    private static (int Status, decimal Amount, string Action) InvokePlanPriceResolver(string planId, decimal amount) { return Escandallo.Core.StripeWebhookCryptoVerifier.ResolveOfficialPlanPrice(planId, amount); }

    private static (int Status, string Verdict) InvokeXmlInspector(string xml) { return Escandallo.Core.OoxmlSecurityInspector.InspectXmlPayloadForXxe(xml); }

    private static string InvokeDdeEscaper(string cell) { return Escandallo.Core.OoxmlSecurityInspector.EscapeSpreadsheetFormula(cell); }

    private static string InvokeLogSanitizer(string key, string val) { return Escandallo.Core.LogSecuritySanitizer.RedactOrSanitizeField(key, val); }

    private static (int Status, string Sql) InvokeRlsBuilder(string centerId) { return Escandallo.Core.RlsSessionCommandBuilder.BuildSetLocalCenterCommand(centerId); }
}
