using Escandallo.Core.Modules.DocIo;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-DOC-IO-001 — Informes Ejecutivos y Seguridad Excel (TSK-005).
/// Requisitos cubiertos: FR-REPORT-001, SEC-REQ-XLSX-001, SEC-REQ-DOS-001.
/// </summary>
public sealed class DocIoReportTests
{
    [Fact]
    public void FrReport001_GeneratesExecutiveReportInPdfAndExcelWithFullBreakdown() {
        // Exportación ejecutiva nominal en PDF y Excel con las 9 columnas financieras
        var pdfReport = DocumentIoService.ExportExecutiveReport(new ExecutiveReportInput(
            "Centro Sol", "active", 3, "pdf", 0));
        var xlsxReport = DocumentIoService.ExportExecutiveReport(new ExecutiveReportInput(
            "Centro Sol", "none", 2, "xlsx", 0));
        Assert.Equal(200, pdfReport.HttpStatus);
        Assert.Equal(3, pdfReport.ExportedRowsCount);
        Assert.NotNull(pdfReport.IncludedColumns);
        Assert.Contains("Escandallo Total", pdfReport.IncludedColumns!);
        Assert.Contains("Beneficio Neto", pdfReport.IncludedColumns!);
        Assert.Contains("Semaforo de Margen", pdfReport.IncludedColumns!);
        Assert.Equal(200, xlsxReport.HttpStatus);
    }

    [Fact]
    public void FrReport001_RejectsUnsupportedFormatsZeroServicesAndConcurrentExportFloods() {
        // Rechazo de formatos no admitidos (400), 0 servicios (422), bloqueo por cuota (402) y DoS (429)
        var unsupported = DocumentIoService.ExportExecutiveReport(new ExecutiveReportInput(
            "Centro Sol", "active", 2, "docx", 0));
        var emptyCatalog = DocumentIoService.ExportExecutiveReport(new ExecutiveReportInput(
            "Centro Sol", "active", 0, "pdf", 0));
        var quotaLocked = DocumentIoService.ExportExecutiveReport(new ExecutiveReportInput(
            "Centro Sol", "canceled", 4, "pdf", 0));
        var floodBlocked = DocumentIoService.ExportExecutiveReport(new ExecutiveReportInput(
            "Centro Sol", "active", 2, "pdf", 2));
        Assert.Equal(400, unsupported.HttpStatus);
        Assert.Equal(422, emptyCatalog.HttpStatus);
        Assert.Equal(402, quotaLocked.HttpStatus);
        Assert.Equal(429, floodBlocked.HttpStatus);
        Assert.Equal("5", floodBlocked.RetryAfterHeader);
    }

    [Fact]
    public void SecReqXlsx001_BlocksZipBombsXxeAndNeutralizesCsvFormulaInjection() {
        // Mitigación de ABUSE-XLSX-INJECT-001: inspección de cabeceras ZIP, bloqueo XXE y prefijo '
        var zipBomb = DocumentIoService.InspectAndParseExcelCatalog(new ExcelArchiveInspectionInput(
            500_000L, 45_000_000L, false, 100));
        var xxePayload = DocumentIoService.InspectAndParseExcelCatalog(new ExcelArchiveInspectionInput(
            120_000L, 400_000L, true, 50));
        var sanitizedDde = DocumentIoService.SanitizeSpreadsheetCell("=cmd|'/C calc'!A0");
        Assert.Equal(422, zipBomb.HttpStatus);
        Assert.Equal("SECURITY", zipBomb.AuditSeverity);
        Assert.Equal(422, xxePayload.HttpStatus);
        Assert.Equal("'=cmd|'/C calc'!A0", sanitizedDde.SanitizedValue);
        Assert.Equal("SANITIZADO", sanitizedDde.Status);
    }
}
