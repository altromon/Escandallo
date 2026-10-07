using System.Linq;
using Escandallo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Escandallo.Web;

public record BillingCheckoutRequest(string CenterId, string PlanId, decimal ClientDeclaredAmount);
public record XlsxInspectRequest(string XmlPayload, string RawCellText);

public static class EscandalloExportRoutes
{
    public static void MapDocumentAndBillingEndpoints(WebApplication app) {
        app.MapGet("/api/reports/pdf", (string? centerId, string? serviceId) => ExportPdf(centerId, serviceId));
        app.MapGet("/api/reports/xlsx", (string? centerId, string? serviceId) => ExportXlsx(centerId, serviceId));
        app.MapPost("/api/billing/checkout", (BillingCheckoutRequest req) => HandleCheckout(req));
        app.MapPost("/api/catalog/inspect-xlsx", (XlsxInspectRequest req) => HandleXlsxInspect(req));
    }

    private static IResult HandleCheckout(BillingCheckoutRequest req) {
        bool validPlan = req.PlanId is "PLAN_MENSUAL_EUR" or "PLAN_ANUAL_EUR";
        int status = validPlan ? 200 : 400;
        if (validPlan && !string.IsNullOrEmpty(req.CenterId)) {
            var t = MultiTenantAppStore.GetCenter(req.CenterId);
            MultiTenantAppStore.SaveCenter(t with { SubscriptionPlan = "PRO" });
        }
        return Results.Json(new { Action = validPlan ? "STRIPE_SESSION_OK" : "PLAN_INVALIDO", Status = status }, statusCode: status);
    }

    private static IResult HandleXlsxInspect(XlsxInspectRequest req) {
        var (httpStatus, verdict) = OoxmlSecurityInspector.InspectXmlPayloadForXxe(req.XmlPayload ?? "<workbook/>");
        var escaped = OoxmlSecurityInspector.EscapeSpreadsheetFormula(req.RawCellText ?? string.Empty);
        return Results.Json(new { HttpStatus = httpStatus, Verdict = verdict, EscapedCell = escaped }, statusCode: httpStatus);
    }

    private static IResult ExportPdf(string? centerId, string? serviceId) {
        var (t, svc, calc, active) = Resolve(centerId, serviceId);
        if (StripeBillingQuotaService.AuthorizeEscandalloOperation(t.SubscriptionPlan == "PRO" ? "active" : "none", active, "exportar_escandallo").HttpStatus == 402) return Results.StatusCode(402);
        return Results.File(RealPdfReportGenerator.BuildPdfBytes(t.CenterName, svc.Name, calc, svc.PvpComercialFijado, t.Parameters), "application/pdf", $"Escandallo_{svc.Id}.pdf");
    }

    private static IResult ExportXlsx(string? centerId, string? serviceId) {
        var (t, svc, calc, active) = Resolve(centerId, serviceId);
        if (StripeBillingQuotaService.AuthorizeEscandalloOperation(t.SubscriptionPlan == "PRO" ? "active" : "none", active, "exportar_escandallo").HttpStatus == 402) return Results.StatusCode(402);
        return Results.File(RealXlsxReportGenerator.BuildXlsxBytes(t.CenterName, svc.Name, calc, svc.PvpComercialFijado, t.Parameters), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Escandallo_{svc.Id}.xlsx");
    }

    private static (TenantCatalogDto T, EscandalloServiceRecord Svc, EscandalloCalculationResult Calc, int Active) Resolve(string? cId, string? sId) {
        var t = MultiTenantAppStore.GetCenter(string.IsNullOrEmpty(cId) ? "center-madrid-01" : cId);
        var svc = t.Escandallos.FirstOrDefault(e => e.Id == sId) ?? t.Escandallos[0];
        return (t, svc, MultiTenantSeedCatalog.ComputeService(t, svc), t.Escandallos.Count(e => !e.Archived));
    }
}
