using Escandallo.Core.Modules.Observability;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-OBS-TELEMETRY-001 — Observabilidad y Redactado (TSK-007).
/// Requisitos cubiertos: FR-OBS-001, SEC-REQ-OBS-001.
/// </summary>
public sealed class ObsTelemetryTests
{
    [Fact]
    public void FrObs001_AllowsMaintainerToSwitchHotLogLevelAndExportTelemetryBundle() {
        // Escenario nominal: ajuste dinámico a DEBUG sin reinicio y exportación NDJSON
        var levelChange = TelemetryService.SetHotLogLevel(new HotLogLevelInput("ACT-MAINTAINER-001", "DEBUG"));
        var bundle = TelemetryService.ExportTelemetryBundle(new TelemetryBundleInput("ACT-MAINTAINER-001", "logs", 24));
        Assert.Equal(200, levelChange.HttpStatus);
        Assert.True(levelChange.AppliedWithoutRestart);
        Assert.Equal(200, bundle.HttpStatus);
        Assert.Equal("ndjson", bundle.OutputFormat);
    }

    [Fact]
    public void FrObs001_EnforcesWindowBoundariesAndBlocksUnprivilegedRoles() {
        // Control RBAC estricto para recursos de telemetría (403 para usuario estético normal)
        var metricsExport = TelemetryService.ExportTelemetryBundle(
            new TelemetryBundleInput("ACT-MAINTAINER-001", "metrics", 168));
        var forbiddenExport = TelemetryService.ExportTelemetryBundle(
            new TelemetryBundleInput("ACT-ESTHETIC-USER-001", "logs", 24));
        Assert.Equal(200, metricsExport.HttpStatus);
        Assert.Equal("prometheus_csv", metricsExport.OutputFormat);
        Assert.Equal(403, forbiddenExport.HttpStatus);
        Assert.Equal("MAINTAINER_ROLE_REQUIRED", forbiddenExport.ErrorCode);
    }

    [Fact]
    public void SecReqObs001_RedactsTokensCookiesAndSignaturesInDebugAndPreventsCrlfInjection() {
        // Mitigación de ABUSE-PRIV-OBS-001: filtro inmutable [REDACTED] y escape de saltos CRLF
        var sanitized = TelemetryService.RedactTelemetryRecord(new RawTelemetryRecordInput(
            "DEBUG",
            "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
            "session_id=sec-token-999",
            "t=170000,v1=deadbeef",
            "servicio\r\n{\"level\":\"INFO\",\"faked\":1}"));
        Assert.Equal("[REDACTED]", sanitized.Authorization);
        Assert.Equal("[REDACTED]", sanitized.Cookie);
        Assert.Equal("[REDACTED]", sanitized.StripeSignature);
        Assert.DoesNotContain("\r\n", sanitized.SingleLineJson);
    }
}
