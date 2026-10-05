using System.Text.Json;
using System.Text.RegularExpressions;

namespace Escandallo.Core.Modules.Observability;

public sealed record HotLogLevelInput(string ActorRole, string NewLevel);

public sealed record HotLogLevelResult(
    int HttpStatus,
    bool AppliedWithoutRestart,
    string? ErrorCode = null);

public sealed record TelemetryBundleInput(
    string ActorRole,
    string Resource,
    int WindowHours);

public sealed record TelemetryBundleResult(
    int HttpStatus,
    string? OutputFormat = null,
    string? ErrorCode = null);

public sealed record RawTelemetryRecordInput(
    string LogLevel,
    string Authorization,
    string Cookie,
    string StripeSignature,
    string SearchQuery);

public sealed record RedactedTelemetryRecordResult(
    string Authorization,
    string Cookie,
    string StripeSignature,
    string SingleLineJson);

public static class TelemetryService
{
    private static readonly HashSet<string> ValidLogLevels = new(StringComparer.Ordinal)
    {
        "DEBUG", "INFO", "WARN", "ERROR"
    };

    private static readonly Regex CrlfRegex = new(@"[\r\n]+", RegexOptions.Compiled);

    public static HotLogLevelResult SetHotLogLevel(HotLogLevelInput input) {
        // Permite al rol ACT-MAINTAINER-001 cambiar el nivel de log en caliente sin reinicio
        if (!string.Equals(input.ActorRole, "ACT-MAINTAINER-001", StringComparison.Ordinal))
        {
            return new HotLogLevelResult(403, false, "MAINTAINER_ROLE_REQUIRED");
        }
        if (!ValidLogLevels.Contains(input.NewLevel))
        {
            return new HotLogLevelResult(400, false, "INVALID_LOG_LEVEL");
        }
        return new HotLogLevelResult(200, true);
    }

    public static TelemetryBundleResult ExportTelemetryBundle(TelemetryBundleInput input) {
        // Exporta logs (ndjson), trazas (otlp_json) y métricas (prometheus_csv) para el mantenedor
        if (!string.Equals(input.ActorRole, "ACT-MAINTAINER-001", StringComparison.Ordinal))
        {
            return new TelemetryBundleResult(403, null, "MAINTAINER_ROLE_REQUIRED");
        }
        if (input.WindowHours < 1 || input.WindowHours > 720)
        {
            return new TelemetryBundleResult(422, null, "WINDOW_HOURS_OUT_OF_RANGE");
        }
        var format = "ndjson";
        if (string.Equals(input.Resource, "traces", StringComparison.Ordinal)) format = "otlp_json";
        if (string.Equals(input.Resource, "metrics", StringComparison.Ordinal)) format = "prometheus_csv";
        return new TelemetryBundleResult(200, format);
    }

    public static RedactedTelemetryRecordResult RedactTelemetryRecord(RawTelemetryRecordInput input) {
        // Ofusca secretos con [REDACTED] incluso en DEBUG y neutraliza saltos CRLF (SEC-REQ-OBS-001)
        var sanitizedQuery = CrlfRegex.Replace(input.SearchQuery, " ");
        var payload = new Dictionary<string, string>
        {
            ["level"] = input.LogLevel,
            ["authorization"] = "[REDACTED]",
            ["cookie"] = "[REDACTED]",
            ["stripe_signature"] = "[REDACTED]",
            ["searchQuery"] = sanitizedQuery
        };
        var singleLine = JsonSerializer.Serialize(payload);
        return new RedactedTelemetryRecordResult("[REDACTED]", "[REDACTED]", "[REDACTED]", singleLine);
    }
}
