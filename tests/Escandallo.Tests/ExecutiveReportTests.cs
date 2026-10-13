using Xunit;

namespace Escandallo.Tests;

/// <summary>
/// Suite de pruebas unitarias y BDD en estado ROJO (FAILING) para FR-REPORT-001, SEC-REQ-XLSX-001 y SEC-REQ-DOS-001.
/// Verifica la exportación exclusiva en PDF/XLSX (PDFsharp/ClosedXML), pre-inspección anti-XXE/ZipBomb y escape DDE.
/// </summary>
public sealed class ExecutiveReportTests
{
    /// <summary>
    /// Escenario Nominal (FR-REPORT-001): Exportación de informe ejecutivo en PDF y Excel dentro de la cuota gratuita.
    /// </summary>
    [Theory]
    [InlineData("Tratamiento Facial", 5.00, 11.25, 3.75, "pdf", 200)]
    [InlineData("Dermapen Completo", 5.00, 18.00, 17.00, "xlsx", 200)]
    [InlineData("Tratamiento Facial", 5.00, 11.25, 3.75, "csv", 400)]
    [Trait("Requirement", "FR-REPORT-001")]
    public void ExportExecutiveReport_SupportedAndUnsupportedFormats_ReturnsExpectedHttpStatus(
        string serviceName,
        decimal overheadCost,
        decimal staffCost,
        decimal productCost,
        string format,
        int expectedHttpStatus)
    {
        // Act: Estado ROJO hasta implementar CMP-DOC-IO-001
        int actualStatus = InvokeUnimplementedReportExport(
            serviceName, overheadCost, staffCost, productCost, format, activeCount: 2);

        // Assert
        Assert.Equal(expectedHttpStatus, actualStatus);
    }

    /// <summary>
    /// Análisis de Frontera (FR-REPORT-001 / SEC-REQ-XLSX-001): Límites de archivo OOXML (2MB, 10MB, ratio 20:1, 5000 filas).
    /// </summary>
    [Theory]
    [InlineData(2047, 10239, 19, 4999, "ARCHIVO_OOXML_VALIDO", 200)]
    [InlineData(2048, 10240, 20, 5000, "ARCHIVO_OOXML_VALIDO", 200)]
    [InlineData(2049, 5000, 5, 1000, "RECHAZADO_TAMANO_COMPRIMIDO", 422)]
    [InlineData(1000, 10241, 11, 1000, "RECHAZADO_RATIO_O_DESCOMPRIMIDO", 422)]
    [InlineData(500, 9000, 21, 100, "RECHAZADO_RATIO_O_DESCOMPRIMIDO", 422)]
    [InlineData(500, 2000, 4, 5001, "RECHAZADO_LIMITE_FILAS_EXCEDIDO", 422)]
    [Trait("Requirement", "SEC-REQ-XLSX-001")]
    public void InspectXlsxArchive_BoundarySizeAndRowLimits_ReturnsExpectedVerdictAndHttpStatus(
        int compressedKb,
        int uncompressedKb,
        int zipRatio,
        int rowCount,
        string expectedVerdict,
        int expectedHttpStatus)
    {
        // Act: Estado ROJO
        (string verdict, int status) = InvokeUnimplementedXlsxInspector(
            compressedKb, uncompressedKb, zipRatio, rowCount, hasExternalDtd: false);

        // Assert
        Assert.Equal(expectedVerdict, verdict);
        Assert.Equal(expectedHttpStatus, status);
    }

    /// <summary>
    /// Escenarios Fuera de Rango / Mitigación de Seguridad (SEC-REQ-XLSX-001): Neutralización de fórmulas DDE y XXE.
    /// </summary>
    [Theory]
    [InlineData("=CMD|'/C calc'!A0", "25.00", "escapar_prefijo_como_texto_literal", "SANITIZADO")]
    [InlineData("+HYPERLINK(\"http://evil.tld\")", "18.50", "escapar_prefijo_como_texto_literal", "SANITIZADO")]
    [InlineData("@SUM(1+1)*cmd", "12.00", "escapar_prefijo_como_texto_literal", "SANITIZADO")]
    [InlineData("Crema Hidratante Facial", "NaN", "rechazar_entrada_aritmetica_invalida", "HTTP_422")]
    [InlineData("Vial Acido Hialuronico", "-15.00", "rechazar_entrada_aritmetica_invalida", "HTTP_422")]
    [InlineData("Aparatologia Laser Diodo", "99999999999.99", "rechazar_desbordamiento_decimal_128bit", "HTTP_422")]
    [Trait("Requirement", "SEC-REQ-XLSX-001")]
    public void SanitizeXlsxCellAndNumericValue_MaliciousPayloads_NeutralizesDdeAndRejectsInvalidNumbers(
        string rawInputName,
        string rawNumericValue,
        string expectedSecurityAction,
        string expectedResultState)
    {
        // Act: Estado ROJO
        (string action, string state) = InvokeUnimplementedDdeSanitizer(rawInputName, rawNumericValue);

        // Assert
        Assert.Equal(expectedSecurityAction, action);
        Assert.Equal(expectedResultState, state);
    }

    /// <summary>
    /// Maquetación Ejecutiva en PDF para Dirección (FR-REPORT-001, CMP-DOC-IO-001 v1.2.0):
    /// Sección 1 con Resumen Ejecutivo inicial (vista colapsada) y Sección 2 con fichas desglosadas por servicio activo.
    /// </summary>
    [Fact]
    [Trait("Requirement", "FR-REPORT-001")]
    public void BuildExecutivePdfReport_ActiveServices_RendersExecutiveSummaryAndDetailedBreakdownForManagement()
    {
        var center = Escandallo.Core.MultiTenantSeedCatalog.CreateDefaultCenter("center-exec-01", "Centro Estética Norte");
        var svc = center.Escandallos[0];
        var calc = Escandallo.Core.MultiTenantSeedCatalog.ComputeService(center, svc);
        string pdfText = System.Text.Encoding.ASCII.GetString(
            Escandallo.Core.RealPdfReportGenerator.BuildPdfBytes(center.CenterName, svc.Name, calc, svc.PvpComercialFijado, center.Parameters));

        Assert.StartsWith("%PDF-1.4", pdfText);
        Assert.Contains("1. RESUMEN EJECUTIVO", pdfText);
        Assert.Contains("2. DESGLOSE DETALLADO", pdfText);
        Assert.True(pdfText.IndexOf("1. RESUMEN EJECUTIVO", System.StringComparison.Ordinal) < pdfText.IndexOf("2. DESGLOSE DETALLADO", System.StringComparison.Ordinal));
        Assert.Contains("60 min", pdfText);
        Assert.Contains("Tecnico", pdfText);
        Assert.Contains("Rentabilidad optima", pdfText);
        Assert.Contains("Productos con merma", pdfText);
        Assert.Contains("Personal imputado", pdfText);
        Assert.Contains("Gastos Generales", pdfText);
        Assert.Contains("Beneficio Neto Objetivo", pdfText);
        Assert.Contains("Precio sin IVA", pdfText);
        Assert.Contains("Beneficio Neto Real", pdfText);
        Assert.Contains("Cleasing Milk", pdfText);
        Assert.Contains("Colagen Mask", pdfText);
        Assert.DoesNotContain("| OPTIMO", pdfText);
    }

    private static int InvokeUnimplementedReportExport(string service, decimal overhead, decimal staff, decimal prod, string format, int activeCount) { return Escandallo.Core.ExecutiveReportService.ExportExecutiveReport(service, overhead, staff, prod, format, activeCount); }

    private static (string Verdict, int Status) InvokeUnimplementedXlsxInspector(int compKb, int uncompKb, int ratio, int rows, bool hasExternalDtd) { return Escandallo.Core.ExecutiveReportService.InspectXlsxArchive(compKb, uncompKb, ratio, rows, hasExternalDtd); }

    private static (string Action, string State) InvokeUnimplementedDdeSanitizer(string rawName, string rawNumber) { return Escandallo.Core.ExecutiveReportService.SanitizeXlsxCellAndNumericValue(rawName, rawNumber); }
}
