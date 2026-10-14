using System.IO;
using System.IO.Compression;
using System.Text;
using Escandallo.Core;
using Xunit;

namespace Escandallo.Tests;

public class WebPersistenceAndReportBinaryTests
{
    [Fact]
    public void RealPdfAndXlsxGenerators_ProduceValidSafeBinaries_AndRoundtripSeedStore()
    {
        var restored = VerifySeedRoundtrip();
        var svc = restored.Escandallos[0];
        var calc = MultiTenantSeedCatalog.ComputeService(restored, svc);
        var pdfText = Encoding.ASCII.GetString(RealPdfReportGenerator.BuildPdfBytes(restored.CenterName, svc.Name, calc, svc.PvpComercialFijado, restored.Parameters));
        Assert.StartsWith("%PDF-1.4", pdfText);
        Assert.Contains("1. RESUMEN EJECUTIVO", pdfText);
        Assert.Contains("2. DESGLOSE DETALLADO", pdfText);
        Assert.Contains("Productos con merma", pdfText);
        Assert.Contains("Personal imputado", pdfText);
        Assert.Contains("Gastos Generales", pdfText);
        Assert.Contains("Cleasing Milk", pdfText);
        Assert.Contains("Colagen Mask", pdfText);
        Assert.DoesNotContain("| OPTIMO", pdfText);
        using var ms = new MemoryStream(RealXlsxReportGenerator.BuildXlsxBytes(restored.CenterName, svc.Name, calc, svc.PvpComercialFijado, restored.Parameters));
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        using var sr = new StreamReader(zip.GetEntry("xl/worksheets/sheet1.xml")!.Open(), Encoding.UTF8);
        Assert.Equal(200, OoxmlSecurityInspector.InspectXmlPayloadForXxe(sr.ReadToEnd()).HttpStatus);
    }

    private static TenantCatalogDto VerifySeedRoundtrip() {
        var restored = TenantPersistenceRepository.DeserializeTenantCatalog(TenantPersistenceRepository.SerializeTenantCatalog(MultiTenantSeedCatalog.CreateDefaultCenter("center-test", "Centro Demo")));
        Assert.Equal(0.05m, restored.DoseEquivalencesMl["gota"]);
        return restored;
    }
}
