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
        Assert.StartsWith("%PDF-1.4", Encoding.ASCII.GetString(RealPdfReportGenerator.BuildPdfBytes(restored.CenterName, svc.Name, calc, svc.PvpComercialFijado, restored.Parameters)));
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
