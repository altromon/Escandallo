namespace Escandallo.Core;

/// <summary>
/// Sanitizador real Anti-Log-Injection (CRLF) y redactado de claves sensibles en telemetría (SEC-REQ-OBS-001, QR-SEC-001).
/// </summary>
public static class LogSecuritySanitizer
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "authorization",
        "cookie",
        "stripe-signature",
        "stripe_signature",
        "credential_hash",
        "password",
        "secret",
        "iban"
    };

    public static string SanitizeLogEntryCrlf(string rawInput) =>
        rawInput.Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);

    public static string RedactOrSanitizeField(string fieldName, string rawValue) =>
        SensitiveKeys.Contains(fieldName) ? "[REDACTED]" : SanitizeLogEntryCrlf(rawValue);
}
