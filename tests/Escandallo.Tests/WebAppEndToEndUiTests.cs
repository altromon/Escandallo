using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Escandallo.Core;
using Escandallo.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Escandallo.Tests;

public class WebAppEndToEndUiTests
{
    [Fact]
    public async Task LiveWebServer_ServesResponsiveUi_ExecutesStateMutations_AndDownloadsPdfAndXlsx()
    {
        MultiTenantAppStore.ResetToSeed();
        await using var app = CreateTestWebServer("http://127.0.0.1:5189");
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new System.Uri("http://127.0.0.1:5189") };
        await VerifyStaticUiAndCssAsync(client);
        await VerifyStateApiAndPdfXlsxDownloadsAsync(client);
        await app.StopAsync();
    }

    private static WebApplication CreateTestWebServer(string url) {
        var root = Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Escandallo.Web"));
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = root, WebRootPath = Path.Combine(root, "wwwroot") });
        builder.WebHost.UseUrls(url);
        var app = builder.Build();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        EscandalloApiRoutes.MapStateAndCatalogEndpoints(app);
        EscandalloExportRoutes.MapDocumentAndBillingEndpoints(app);
        return app;
    }

    private static async Task VerifyStaticUiAndCssAsync(HttpClient client) {
        var html = await client.GetStringAsync("/index.html");
        Assert.Contains("aria-live=\"polite\"", html);
        Assert.Contains("cascadeModal", html);
        Assert.Contains("paywallModal", html);
        var css = await client.GetStringAsync("/app.css");
        Assert.Contains("min-height: 44px", css);
    }

    private static async Task VerifyStateApiAndPdfXlsxDownloadsAsync(HttpClient client) {
        var stateJson = await client.GetStringAsync("/api/state?centerId=center-madrid-01");
        Assert.Contains("Higiene Facial", stateJson);
        var updated = MultiTenantSeedCatalog.CreateDefaultCenter("center-madrid-01", "Estética Lumière Madrid Editado");
        var saveRes = await client.PostAsJsonAsync("/api/state/save", updated);
        Assert.True(saveRes.IsSuccessStatusCode);
        var pdfBytes = await client.GetByteArrayAsync("/api/reports/pdf?centerId=center-madrid-01&serviceId=esc-001");
        Assert.True(pdfBytes.Length > 100);
        var xlsxBytes = await client.GetByteArrayAsync("/api/reports/xlsx?centerId=center-madrid-01&serviceId=esc-001");
        Assert.True(xlsxBytes.Length > 100);
    }
}
