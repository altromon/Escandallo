using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bunit;
using Escandallo.Core;
using Escandallo.Core.Components;
using Escandallo.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Escandallo.Tests;

public class WebUiInteractiveFlowsE2ETests
{
    [Fact]
    public void CompiledBlazorComponents_RenderValidWaiAriaDom_ViaBunit() {
        using var ctx = new TestContext();
        Assert.Contains("role=\"application\"", ctx.RenderComponent<MainResponsiveLayout>().Markup);
        Assert.Contains("role=\"tablist\"", ctx.RenderComponent<CostCatalogPage>().Markup);
        Assert.Contains("aria-live=\"polite\"", ctx.RenderComponent<EscandalloBuilderPage>().Markup);
        Assert.Contains("role=\"dialog\"", ctx.RenderComponent<CascadeImpactModal>().Markup);
        Assert.Contains("14,90 EUR/mes", ctx.RenderComponent<FreemiumPaywallModal>().Markup);
    }

    [Fact]
    public async Task InteractiveFlows_AllApiEndpoints_DoseAssistant_Stripe_Xlsx_AndObservability_WorkEndToEnd() {
        await using var app = StartServer("http://127.0.0.1:5192");
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new System.Uri("http://127.0.0.1:5192") };
        await VerifyAllIntegratedEndpointsAsync(client);
        await app.StopAsync();
    }

    private static WebApplication StartServer(string url) {
        var root = Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Escandallo.Web"));
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = root, WebRootPath = Path.Combine(root, "wwwroot") });
        builder.WebHost.UseUrls(url);
        var app = builder.Build();
        EscandalloApiRoutes.MapStateAndCatalogEndpoints(app);
        EscandalloExportRoutes.MapDocumentAndBillingEndpoints(app);
        return app;
    }

    private static async Task VerifyAllIntegratedEndpointsAsync(HttpClient client) {
        var t = MultiTenantSeedCatalog.CreateDefaultCenter("center-e2e-flows", "Centro E2E");
        t.Escandallos[0] = t.Escandallos[0] with { PvpComercialFijado = 22.00m };
        t.Escandallos[1] = t.Escandallos[1] with { PvpComercialFijado = 12.00m };
        await client.PostAsJsonAsync("/api/state/save", t);
        var state = await client.GetStringAsync("/api/state?centerId=center-e2e-flows");
        Assert.Contains("ALERTA", state);
        Assert.Contains("CRITICO", state);
        var doseRes = await client.PostAsJsonAsync("/api/dose/resolve", new DoseResolveRequest("pulsacion", 3m, 2.0m));
        Assert.Contains("6", await doseRes.Content.ReadAsStringAsync());
        var billRes = await client.PostAsJsonAsync("/api/billing/checkout", new BillingCheckoutRequest("center-e2e-flows", "PLAN_MENSUAL_EUR", 0.01m));
        Assert.True(billRes.IsSuccessStatusCode);
        var xlsxRes = await client.PostAsJsonAsync("/api/catalog/inspect-xlsx", new XlsxInspectRequest("<workbook/>", "=CMD|' /C calc'!A0"));
        Assert.Contains("'=CMD", await xlsxRes.Content.ReadAsStringAsync());
        var obsRes = await client.PostAsJsonAsync("/api/observability/loglevel", new ObservabilityLogRequest("ACT-MAINTAINER-001", "DEBUG", "Authorization: Bearer sk_live_123456789"));
        Assert.Contains("[REDACTED]", await obsRes.Content.ReadAsStringAsync());
    }
}
