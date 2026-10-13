using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Escandallo.Core;
using Escandallo.Web;
using Microsoft.AspNetCore.Builder;
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
        await VerifyStaticUiMarkupAsync(client);
        await VerifyStaticCssRulesAsync(client);
        await VerifyStateApiAndIsolationAsync(client);
        await VerifyExecutivePdfAndXlsxDownloadsAsync(client);
        await app.StopAsync();
    }

    private static WebApplication CreateTestWebServer(string url) =>
        Program.CreateWebApp([], url);

    private static async Task VerifyStaticUiMarkupAsync(HttpClient client) {
        var html = await client.GetStringAsync("/index.html");
        Assert.Contains("authGateSection", html);
        Assert.Contains("id=\"escandallosSection\"", html);
        Assert.Contains("point-1-full-width", html);
        Assert.Contains("form-row", html);
        Assert.Contains("dose-grid", html);
        Assert.Contains("dose-assistant-box", html);
        Assert.Contains("syncResponsiveViewport", html);
        Assert.Contains("Descargar PDF de todos los servicios", html);
        Assert.Contains("service-collapsed-row", html);
        Assert.Contains("archivedServicesList", html);
        Assert.DoesNotContain("style=\"width:90px;\"", html);
        Assert.True(html.IndexOf("1. Calculadora de servicios", System.StringComparison.Ordinal) < html.IndexOf("2. Catálogo de costes", System.StringComparison.Ordinal));
    }

    private static async Task VerifyStaticCssRulesAsync(HttpClient client) {
        var css = await client.GetStringAsync("/app.css");
        Assert.Contains(".point-1-full-width", css);
        Assert.Contains("max-width: 100%", css);
        Assert.Contains(".service-collapsed-row", css);
        Assert.Contains("@media (max-width: 767px)", css);
        Assert.Contains(".dose-grid", css);
        Assert.Contains(".form-row", css);
    }

    private static async Task VerifyStateApiAndIsolationAsync(HttpClient client) {
        var unauthRes = await client.GetAsync("/api/state?centerId=center-madrid-01&requireAuth=1");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, unauthRes.StatusCode);
        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new AuthLoginRequest("cliente-a@estetica.es", "Estetica2026!"));
        Assert.True(loginRes.IsSuccessStatusCode);
        client.DefaultRequestHeaders.Add("X-Escandallo-User", "cliente-a@estetica.es");
        var crossTenantRes = await client.GetAsync("/api/state?centerId=center-sevilla-03");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, crossTenantRes.StatusCode);
        var stateJson = await client.GetStringAsync("/api/state?centerId=center-madrid-01");
        Assert.Contains("Higiene Facial", stateJson);
        Assert.DoesNotContain("center-sevilla-03", stateJson);
    }

    private static async Task VerifyExecutivePdfAndXlsxDownloadsAsync(HttpClient client) {
        var updated = MultiTenantSeedCatalog.CreateDefaultCenter("center-madrid-01", "Estética Lumière Madrid Editado");
        var saveRes = await client.PostAsJsonAsync("/api/state/save", updated);
        Assert.True(saveRes.IsSuccessStatusCode);
        var pdfBytes = await client.GetByteArrayAsync("/api/reports/pdf?centerId=center-madrid-01");
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfBytes);
        Assert.Contains("1. RESUMEN EJECUTIVO", pdfText);
        Assert.Contains("2. DESGLOSE DETALLADO", pdfText);
        Assert.Contains("Higiene Facial", pdfText);
        Assert.Contains("Peeling", pdfText);
        Assert.Contains("Cleasing Milk", pdfText);
        Assert.Contains("Lifting Peeling", pdfText);
        var xlsxBytes = await client.GetByteArrayAsync("/api/reports/xlsx?centerId=center-madrid-01");
        Assert.True(xlsxBytes.Length > 100);
    }
}
