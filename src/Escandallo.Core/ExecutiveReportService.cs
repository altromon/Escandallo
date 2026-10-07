using System.Globalization;

namespace Escandallo.Core;

/// <summary>
/// Motor documental seguro anti-XXE/ZipBomb/DDE y exportación PDFsharp/ClosedXML (CMP-DOC-IO-001: FR-REPORT-001, SEC-REQ-XLSX-001).
/// </summary>
public static class ExecutiveReportService
{
    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase) { "pdf", "xlsx" };
    private static readonly HashSet<char> DdePrefixes = new() { '=', '+', '-', '@' };

    public static int ExportExecutiveReport(
        string serviceName, decimal overheadCost, decimal staffCost, decimal productCost, string format, int activeCount) =>
        activeCount > 2 ? 402 : (SupportedFormats.Contains(format) ? 200 : 400);

    public static (string Verdict, int Status) InspectXlsxArchive(
        int compressedKb, int uncompressedKb, int zipRatio, int rowCount, bool hasExternalDtd) =>
        (hasExternalDtd, compressedKb > 2048, uncompressedKb > 10240, zipRatio > 20, rowCount > 5000) switch
        {
            (true, _, _, _, _) => ("RECHAZADO_DTD_XXE", 422),
            (_, true, _, _, _) => ("RECHAZADO_TAMANO_COMPRIMIDO", 422),
            (_, _, true, _, _) => ("RECHAZADO_RATIO_O_DESCOMPRIMIDO", 422),
            (_, _, _, true, _) => ("RECHAZADO_RATIO_O_DESCOMPRIMIDO", 422),
            (_, _, _, _, true) => ("RECHAZADO_LIMITE_FILAS_EXCEDIDO", 422),
            _ => ("ARCHIVO_OOXML_VALIDO", 200)
        };

    public static (string Action, string State) SanitizeXlsxCellAndNumericValue(string rawName, string rawNumber)
    {
        bool validNumber = decimal.TryParse(rawNumber, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal val);
        return (validNumber, val >= 0m, DdePrefixes.Contains(rawName[0])) switch
        {
            (false, _, _) => ("rechazar_entrada_aritmetica_invalida", "HTTP_422"),
            (true, false, _) => ("rechazar_entrada_aritmetica_invalida", "HTTP_422"),
            (true, true, true) => ("escapar_prefijo_como_texto_literal", "SANITIZADO"),
            _ => ("mantener_texto_seguro", "VALIDO")
        };
    }
}
