using System.Globalization;
using System.Text;

namespace Escandallo.Core;

public static class RealPdfReportGenerator
{
    public static byte[] BuildPdfBytes(string centerName, string serviceName, EscandalloCalculationResult calc, decimal commercialPvp, GlobalParameters p)
    {
        var sem = EscandalloCalculatorService.EvaluateMarginSemaphore(calc.CosteTotal, p.Beneficio, p.Iva, commercialPvp);
        var txt = string.Format(CultureInfo.InvariantCulture, "BT /F1 12 Tf 50 750 Td (ESCANDALLO - {0} - {1}) Tj 0 -20 Td (Coste: {2:0.00} EUR | PVP Rec: {3:0.00} EUR | PVP Com: {4:0.00} EUR | {5}) Tj ET", Sanitize(centerName), Sanitize(serviceName), calc.CosteTotal, calc.PvpRecomendado, commercialPvp, sem.Semaphore);
        var pdf = $"%PDF-1.4\n1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources<< /Font<< /F1 4 0 R >> >> /Contents 5 0 R >>endobj\n4 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n5 0 obj<< /Length {txt.Length} >>stream\n{txt}\nendstream\nendobj\ntrailer<< /Root 1 0 R >>\n%%EOF";
        return Encoding.ASCII.GetBytes(pdf);
    }

    private static string Sanitize(string s) { return s.Replace("(", "[").Replace(")", "]"); }
}
