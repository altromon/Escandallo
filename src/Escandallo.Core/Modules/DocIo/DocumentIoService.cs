using System.Text.RegularExpressions;

namespace Escandallo.Core.Modules.DocIo;

public sealed record ExecutiveReportInput(
    string CenterName,
    string SubscriptionStatus,
    int ActiveServicesCount,
    string Format,
    int ConcurrentExportsInFlight);

public sealed record ExecutiveReportResult(
    int HttpStatus,
    string? ErrorCode = null,
    string? RetryAfterHeader = null,
    int ExportedRowsCount = 0,
    IReadOnlyList<string>? IncludedColumns = null);

public sealed record ExcelArchiveInspectionInput(
    long CompressedBytes,
    long UncompressedBytes,
    bool ContainsExternalDtdOrEntity,
    int RowCount);

public sealed record ExcelArchiveInspectionResult(
    int HttpStatus,
    int ImportedRows = 0,
    string? AuditSeverity = null);

public sealed record SanitizedCellResult(string SanitizedValue, string Status);

public static class DocumentIoService
{
    private static readonly string[] ExecutiveColumns =
    {
        "Gastos Generales",
        "Coste Persona",
        "Coste de Productos",
        "Escandallo Total",
        "Beneficio Neto",
        "Precio sin IVA",
        "PVP Calculado",
        "PVP Comercial Fijado",
        "Semaforo de Margen"
    };

    private static readonly Regex FormulaInjectionRegex = new(@"^[=+\-@\t\r]", RegexOptions.Compiled);

    private static bool IsExceedingZipLimits(ExcelArchiveInspectionInput input) {
        // Verifica tamaño comprimido <= 2MB, descomprimido <= 10MB y ratio de compresión <= 20:1
        var safeCompressed = Math.Max(1L, input.CompressedBytes);
        var ratio = (double)input.UncompressedBytes / safeCompressed;
        return input.CompressedBytes > 2_000_000L ||
               input.UncompressedBytes > 10_000_000L ||
               ratio > 20.0;
    }

    public static ExecutiveReportResult ExportExecutiveReport(ExecutiveReportInput input) {
        // Valida formato (pdf/xlsx), servicios activos, cuota freemium y semáforo de concurrencia
        if (!string.Equals(input.Format, "pdf", StringComparison.Ordinal) &&
            !string.Equals(input.Format, "xlsx", StringComparison.Ordinal))
        {
            return new ExecutiveReportResult(400, "UNSUPPORTED_EXPORT_FORMAT");
        }
        if (input.ActiveServicesCount <= 0)
        {
            return new ExecutiveReportResult(422, "NO_ACTIVE_SERVICES_TO_EXPORT");
        }
        if (!string.Equals(input.SubscriptionStatus, "active", StringComparison.Ordinal) &&
            input.ActiveServicesCount > 2)
        {
            return new ExecutiveReportResult(402, "QUOTA_EXCEEDED_EXPORT_LOCK");
        }
        if (input.ConcurrentExportsInFlight >= 2)
        {
            return new ExecutiveReportResult(429, null, "5");
        }
        return new ExecutiveReportResult(200, null, null, input.ActiveServicesCount, ExecutiveColumns);
    }

    public static ExcelArchiveInspectionResult InspectAndParseExcelCatalog(ExcelArchiveInspectionInput input) {
        // Bloquea entidades externas XML (XXE), bombas ZIP y hojas con más de 5.000 filas (SEC-REQ-XLSX-001)
        if (input.ContainsExternalDtdOrEntity || IsExceedingZipLimits(input) || input.RowCount > 5000)
        {
            return new ExcelArchiveInspectionResult(422, 0, "SECURITY");
        }
        return new ExcelArchiveInspectionResult(200, input.RowCount);
    }

    public static SanitizedCellResult SanitizeSpreadsheetCell(string rawValue) {
        // Neutraliza fórmulas ejecutables DDE/CSV anteponiendo apóstrofo literal
        if (FormulaInjectionRegex.IsMatch(rawValue))
        {
            return new SanitizedCellResult($"'{rawValue}", "SANITIZADO");
        }
        return new SanitizedCellResult(rawValue, "LIMPIO");
    }
}
