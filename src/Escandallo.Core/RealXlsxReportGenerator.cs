using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Escandallo.Core;

public static class RealXlsxReportGenerator
{
    public static byte[] BuildXlsxBytes(string centerName, string serviceName, EscandalloCalculationResult calc, decimal commercialPvp, GlobalParameters p)
    {
        var sem = EscandalloCalculatorService.EvaluateMarginSemaphore(calc.CosteTotal, p.Beneficio, p.Iva, commercialPvp);
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteXml(zip, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"xml\" ContentType=\"application/xml\"/></Types>");
            WriteXml(zip, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheets><sheet name=\"Escandallo\" sheetId=\"1\"/></sheets></workbook>");
            WriteXml(zip, "xl/worksheets/sheet1.xml", string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData><row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>{0} - {1} ({2})</t></is></c><c r=\"B1\"><v>{3:0.00}</v></c></row></sheetData></worksheet>", OoxmlSecurityInspector.EscapeSpreadsheetFormula(centerName), OoxmlSecurityInspector.EscapeSpreadsheetFormula(serviceName), sem.Semaphore, calc.PvpRecomendado));
        }
        return ms.ToArray();
    }

    private static void WriteXml(ZipArchive zip, string path, string xml) { using var w = new StreamWriter(zip.CreateEntry(path).Open(), Encoding.UTF8); w.Write(xml); }
}
