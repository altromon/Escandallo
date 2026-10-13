using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Escandallo.Core;

public static class RealPdfReportGenerator
{
    public static byte[] BuildPdfBytes(string centerName, string serviceName, EscandalloCalculationResult calc, decimal commercialPvp, GlobalParameters p) {
        var tenant = MultiTenantSeedCatalog.CreateDefaultCenter("center-exec", centerName) with { Parameters = p };
        tenant.Escandallos[0] = tenant.Escandallos[0] with { Name = serviceName, PvpComercialFijado = commercialPvp };
        return BuildExecutiveCenterPdfBytes(tenant, calc);
    }

    public static byte[] BuildExecutiveCenterPdfBytes(TenantCatalogDto tenant, EscandalloCalculationResult? overrideFirstCalc = null) {
        var active = tenant.Escandallos.Where(e => !e.Archived).ToList();
        var lines = new List<string> {
            $"INFORME EJECUTIVO PARA DIRECCION - {Sanitize(tenant.CenterName)}",
            string.Format(CultureInfo.InvariantCulture, "Parametros Vigentes -> IVA: {0:0}% | Beneficio Objetivo: {1:0}% | Merma: {2:0}%", tenant.Parameters.Iva * 100m, tenant.Parameters.Beneficio * 100m, tenant.Parameters.MermaPct * 100m),
            "1. RESUMEN EJECUTIVO - TABLA CONSOLIDADA DE SERVICIOS ACTIVOS (VISTA COLAPSADA)",
            "Servicio | Duracion | Personal | Coste Total | Precio Venta | Precio Recomendado | Estado de Rentabilidad"
        };
        AppendSummaryRows(tenant, active, overrideFirstCalc, lines);
        lines.Add("2. DESGLOSE DETALLADO DE CADA SERVICIO PARA DIRECCION");
        AppendDetailedServiceCards(tenant, active, overrideFirstCalc, lines);
        return EncodePdfDocument(lines);
    }

    private static void AppendSummaryRows(TenantCatalogDto tenant, List<EscandalloServiceRecord> active, EscandalloCalculationResult? overrideFirstCalc, List<string> lines) {
        for (int i = 0; i < active.Count; i++) {
            var svc = active[i];
            var calc = i == 0 && overrideFirstCalc is not null ? overrideFirstCalc : MultiTenantSeedCatalog.ComputeService(tenant, svc);
            var sem = EscandalloCalculatorService.EvaluateMarginSemaphore(calc.CosteTotal, tenant.Parameters.Beneficio, tenant.Parameters.Iva, svc.PvpComercialFijado);
            lines.Add(string.Format(CultureInfo.InvariantCulture, "{0} | {1} min | {2} | {3:0.00} EUR | {4:0.00} EUR | {5:0.00} EUR | {6}",
                Sanitize(svc.Name), svc.DuracionMinutos, Sanitize(svc.StaffRole), calc.CosteTotal, svc.PvpComercialFijado, calc.PvpRecomendado, ResolveExecutiveSemaphoreText(sem.Semaphore)));
        }
    }

    private static void AppendDetailedServiceCards(TenantCatalogDto tenant, List<EscandalloServiceRecord> active, EscandalloCalculationResult? overrideFirstCalc, List<string> lines) {
        for (int i = 0; i < active.Count; i++) {
            var svc = active[i];
            var calc = i == 0 && overrideFirstCalc is not null ? overrideFirstCalc : MultiTenantSeedCatalog.ComputeService(tenant, svc);
            var sem = EscandalloCalculatorService.EvaluateMarginSemaphore(calc.CosteTotal, tenant.Parameters.Beneficio, tenant.Parameters.Iva, svc.PvpComercialFijado);
            decimal realMarginPct = calc.CosteTotal > 0m ? (sem.NetProfit / calc.CosteTotal) * 100m : 0m;
            lines.Add(string.Format(CultureInfo.InvariantCulture, "FICHA EJECUTIVA: {0} ({1} min - Personal: {2}) - Dictamen: {3}", Sanitize(svc.Name), svc.DuracionMinutos, Sanitize(svc.StaffRole), ResolveExecutiveSemaphoreText(sem.Semaphore)));
            lines.Add(string.Format(CultureInfo.InvariantCulture, "Costes -> Productos con merma: {0:0.00} EUR | Personal imputado: {1:0.00} EUR | Gastos Generales: {2:0.00} EUR | Escandallo Total: {3:0.00} EUR", calc.CosteProductos, calc.CostePersonal, calc.CosteGastosGenerales, calc.CosteTotal));
            lines.Add(string.Format(CultureInfo.InvariantCulture, "Rentabilidad -> Beneficio Neto Objetivo: {0:0.00} EUR | Precio sin IVA: {1:0.00} EUR | PVP Recomendado: {2:0.00} EUR | PVP Comercial: {3:0.00} EUR | Beneficio Neto Real: {4:0.00} EUR ({5:0.0}%)", calc.BeneficioNeto, sem.PriceExVat, calc.PvpRecomendado, svc.PvpComercialFijado, sem.NetProfit, realMarginPct));
            AppendRecipeBreakdown(tenant, svc, lines);
        }
    }

    private static void AppendRecipeBreakdown(TenantCatalogDto tenant, EscandalloServiceRecord svc, List<string> lines) {
        foreach (var item in svc.Recipe) {
            var prod = tenant.Products.FirstOrDefault(p => p.Name == item.ProductName);
            decimal cost = prod is null ? 0m : Math.Round((prod.PrecioCompra / prod.CantidadMl) * item.CantidadServicioMl * (1m + tenant.Parameters.MermaPct), 2, MidpointRounding.AwayFromZero);
            lines.Add(string.Format(CultureInfo.InvariantCulture, "  Receta -> {0}: {1:0.##} ml (Coste imputado con merma: {2:0.00} EUR)", Sanitize(item.ProductName), item.CantidadServicioMl, cost));
        }
    }

    private static byte[] EncodePdfDocument(List<string> lines) {
        var streamRows = string.Join("\n", lines.Select((line, idx) => idx == 0 ? $"({line}) Tj" : $"0 -14 Td ({line}) Tj"));
        var txt = $"BT /F1 9 Tf 36 800 Td\n{streamRows}\nET";
        var pdf = $"%PDF-1.4\n1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources<< /Font<< /F1 4 0 R >> >> /Contents 5 0 R >>endobj\n4 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n5 0 obj<< /Length {txt.Length} >>stream\n{txt}\nendstream\nendobj\ntrailer<< /Root 1 0 R >>\n%%EOF";
        return Encoding.ASCII.GetBytes(pdf);
    }

    private static string ResolveExecutiveSemaphoreText(string semaphore) {
        return semaphore == "OPTIMO" ? "Rentabilidad optima" : (semaphore == "ALERTA" ? "Margen ajustado" : "Atencion: perdida");
    }

    private static string Sanitize(string s) { return s.Replace("(", "[").Replace(")", "]"); }
}
