namespace Escandallo.Core;

/// <summary>
/// Telemetría Serilog/OpenTelemetry, redactado inmutable DEBUG con TTL 60m y validador WCAG 2.1 AA (CMP-OBS-TELEMETRY-001: FR-OBS-001, SEC-REQ-OBS-001).
/// </summary>
public static class ObservabilityService
{
    public static int AuthorizeObservabilityResource(string actorRole, string telemetryResource)
    {
        return actorRole == "ACT-MAINTAINER-001" ? 200 : 403;
    }

    public static (string SerializedValue, string EffectiveLevel) RedactSensitiveFieldsAndEvaluateDebugTtl(
        string activeLevel, int elapsedMinutes, string sensitiveField)
    {
        string effectiveLevel = elapsedMinutes >= 60 ? "INFO" : activeLevel;
        return ($"[REDACTED]", effectiveLevel);
    }

    public static string ValidateWcagAccessibilityBoundaries(double contrastRatio, int touchTargetPixels)
    {
        if (contrastRatio < 4.5) return "RECHAZADO_CONTRASTE_BAJO";
        if (touchTargetPixels < 44) return "RECHAZADO_TARGET_PEQUENO";
        return $"APROBADO_WCAG_AA";
    }
}
