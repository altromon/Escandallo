using System;
using System.Linq;
using Escandallo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Escandallo.Web;

public record DoseResolveRequest(string UnitType, decimal UnitsCount, decimal? CustomMl);
public record ObservabilityLogRequest(string ActorRole, string TargetLevel, string SampleLogLine);

public static class EscandalloApiRoutes
{
    public static void MapStateAndCatalogEndpoints(WebApplication app) {
        app.MapGet("/api/state", (string? centerId) => BuildStateResponse(centerId ?? "center-madrid-01"));
        app.MapPost("/api/state/save", (TenantCatalogDto updated) => Results.Ok(MultiTenantAppStore.SaveCenter(updated)));
        app.MapPost("/api/dose/resolve", (DoseResolveRequest req) => Results.Ok(new { Ml = CabinDoseEquivalenceService.ResolveDoseInMlOrGrams(req.UnitType, req.UnitsCount, req.CustomMl) }));
        app.MapPost("/api/observability/loglevel", (ObservabilityLogRequest req) => HandleObservability(req));
    }

    private static IResult HandleObservability(ObservabilityLogRequest req) {
        int status = ObservabilityService.AuthorizeObservabilityResource(req.ActorRole, "loglevel");
        var (masked, effective) = ObservabilityService.RedactSensitiveFieldsAndEvaluateDebugTtl(req.TargetLevel, 15, "authorization");
        var sanitized = LogSecuritySanitizer.RedactOrSanitizeField("authorization", req.SampleLogLine ?? masked);
        return Results.Json(new { State = effective, Ttl = "TTL_60M", Status = status, SanitizedLog = sanitized }, statusCode: status);
    }

    private static IResult BuildStateResponse(string centerId) {
        var t = MultiTenantAppStore.GetCenter(centerId);
        var p = t.Parameters;
        var ovhRate = CostCatalogService.CalculateOverheadMinuteRate(t.Overheads.Sum(o => o.ImporteMensual), p.MinutosMensuales);
        var prods = t.Products.ToDictionary(x => x.Name, x => new { CosteConMerma = Math.Round((x.PrecioCompra / x.CantidadMl) * x.CantidadPorServicioMl * (1m + p.MermaPct), 4, MidpointRounding.AwayFromZero) });
        var stf = t.Staff.ToDictionary(s => s.Role, s => new { CosteMinuto = CostCatalogService.CalculateUnitCost(s.Role, "personal", p.MinutosMensuales, s.SueldoMensual, 0m, p.CoeficienteEmpresa).UnitCost });
        var list = t.Escandallos.Select(e => {
            var calc = MultiTenantSeedCatalog.ComputeService(t, e);
            var sem = EscandalloCalculatorService.EvaluateMarginSemaphore(calc.CosteTotal, p.Beneficio, p.Iva, e.PvpComercialFijado);
            return new { Record = e, Calculation = calc, Semaphore = new { Status = sem.Semaphore, NetProfit = sem.NetProfit, PriceExVat = sem.PriceExVat, RealMarginRate = calc.CosteTotal > 0 ? sem.NetProfit / calc.CosteTotal : 0m } };
        }).ToList();
        return Results.Ok(new { Tenant = t, Catalog = new { OverheadRatePerMinute = ovhRate, Products = prods, Staff = stf }, Escandallos = list, ActiveCount = t.Escandallos.Count(e => !e.Archived), Centers = MultiTenantAppStore.ListCentersSummary() });
    }
}
