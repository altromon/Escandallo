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
        var sb = new StringBuilder();
        int y = 808;
        y = RenderExecutiveHeader(sb, tenant, y);
        y = RenderSummaryTable(sb, tenant, active, overrideFirstCalc, y);
        RenderServiceCards(sb, tenant, active, overrideFirstCalc, y);
        RenderPageFooter(sb, 1, 1);
        return EncodePdfDocument(sb.ToString());
    }

    private static int RenderExecutiveHeader(StringBuilder sb, TenantCatalogDto tenant, int startY) {
        DrawFillRect(sb, 0, 836, 595, 6, "0.12 0.25 0.69");
        DrawText(sb, "F2", 15, "0.12 0.25 0.69", 36, startY, $"INFORME EJECUTIVO PARA DIRECCION - {Sanitize(tenant.CenterName)}");
        DrawText(sb, "F1", 9, "0.33 0.41 0.52", 36, startY - 14, "Centro de Estetica | Informe Oficial de Rentabilidad y Escandallos");
        DrawFilledBox(sb, 36, startY - 42, 523, 22, "0.95 0.97 1.0", "0.78 0.84 0.93");
        var paramText = string.Format(CultureInfo.InvariantCulture, "Parametros Vigentes -> IVA: {0:0}% | Beneficio Objetivo: {1:0}% | Merma: {2:0}%", tenant.Parameters.Iva * 100m, tenant.Parameters.Beneficio * 100m, tenant.Parameters.MermaPct * 100m);
        DrawText(sb, "F2", 8.5, "0.12 0.25 0.69", 44, startY - 35, paramText);
        return startY - 58;
    }

    private static int RenderSummaryTable(StringBuilder sb, TenantCatalogDto tenant, List<EscandalloServiceRecord> active, EscandalloCalculationResult? overrideFirstCalc, int y) {
        DrawFillRect(sb, 36, y - 2, 4, 14, "0.12 0.25 0.69");
        DrawText(sb, "F2", 10.5, "0.06 0.09 0.16", 45, y, "1. RESUMEN EJECUTIVO - TABLA CONSOLIDADA DE SERVICIOS ACTIVOS (VISTA COLAPSADA)");
        y -= 20;
        DrawFillRect(sb, 36, y - 18, 523, 18, "0.12 0.25 0.69");
        DrawText(sb, "F2", 7.5, "1.0 1.0 1.0", 42, y - 12, "Servicio | Duracion | Personal | Coste Total | Precio Venta | Precio Recomendado | Estado de Rentabilidad");
        y -= 18;
        for (int i = 0; i < active.Count; i++) {
            y = RenderSummaryTableRow(sb, tenant, active[i], i, i == 0 ? overrideFirstCalc : null, y);
        }
        DrawStrokeRect(sb, 36, y, 523, (active.Count * 20) + 18, "0.78 0.84 0.93");
        return y - 16;
    }

    private static int RenderSummaryTableRow(StringBuilder sb, TenantCatalogDto tenant, EscandalloServiceRecord svc, int idx, EscandalloCalculationResult? overrideCalc, int y) {
        var calc = overrideCalc ?? MultiTenantSeedCatalog.ComputeService(tenant, svc);
        var sem = EscandalloCalculatorService.EvaluateMarginSemaphore(calc.CosteTotal, tenant.Parameters.Beneficio, tenant.Parameters.Iva, svc.PvpComercialFijado);
        string bg = idx % 2 == 0 ? "0.98 0.99 1.0" : "1.0 1.0 1.0";
        DrawFillRect(sb, 36, y - 20, 523, 20, bg);
        DrawLine(sb, 36, y - 20, 559, y - 20, "0.88 0.91 0.95", 0.5);
        DrawText(sb, "F2", 8, "0.06 0.09 0.16", 42, y - 13, Sanitize(svc.Name));
        DrawText(sb, "F1", 8, "0.20 0.25 0.33", 160, y - 13, $"{svc.DuracionMinutos} min");
        DrawText(sb, "F1", 8, "0.20 0.25 0.33", 215, y - 13, Sanitize(svc.StaffRole));
        DrawText(sb, "F1", 8, "0.20 0.25 0.33", 270, y - 13, string.Format(CultureInfo.InvariantCulture, "{0:0.00} EUR", calc.CosteTotal));
        DrawText(sb, "F2", 8, "0.12 0.25 0.69", 335, y - 13, string.Format(CultureInfo.InvariantCulture, "{0:0.00} EUR", svc.PvpComercialFijado));
        DrawText(sb, "F1", 8, "0.20 0.25 0.33", 400, y - 13, string.Format(CultureInfo.InvariantCulture, "{0:0.00} EUR", calc.PvpRecomendado));
        DrawSemaphoreBadge(sb, 470, y - 16, 85, 14, sem.Semaphore);
        return y - 20;
    }

    private static void RenderServiceCards(StringBuilder sb, TenantCatalogDto tenant, List<EscandalloServiceRecord> active, EscandalloCalculationResult? overrideFirstCalc, int y) {
        DrawFillRect(sb, 36, y - 2, 4, 14, "0.12 0.25 0.69");
        DrawText(sb, "F2", 10.5, "0.06 0.09 0.16", 45, y, "2. DESGLOSE DETALLADO DE CADA SERVICIO PARA DIRECCION");
        y -= 16;
        for (int i = 0; i < active.Count; i++) {
            var svc = active[i];
            var calc = i == 0 && overrideFirstCalc is not null ? overrideFirstCalc : MultiTenantSeedCatalog.ComputeService(tenant, svc);
            var sem = EscandalloCalculatorService.EvaluateMarginSemaphore(calc.CosteTotal, tenant.Parameters.Beneficio, tenant.Parameters.Iva, svc.PvpComercialFijado);
            y = RenderSingleServiceCard(sb, tenant, svc, calc, sem, y);
        }
    }

    private static int RenderSingleServiceCard(StringBuilder sb, TenantCatalogDto tenant, EscandalloServiceRecord svc, EscandalloCalculationResult calc, (decimal PriceExVat, decimal NetProfit, string Semaphore) sem, int y) {
        int cardH = 70 + (svc.Recipe.Count * 13);
        DrawFilledBox(sb, 36, y - cardH, 523, cardH, "0.98 0.99 1.0", "0.80 0.85 0.92");
        string stripeColor = sem.Semaphore == "OPTIMO" ? "0.09 0.64 0.29" : (sem.Semaphore == "ALERTA" ? "0.79 0.54 0.02" : "0.86 0.15 0.15");
        DrawFillRect(sb, 36, y - cardH, 5, cardH, stripeColor);
        DrawFillRect(sb, 41, y - 18, 518, 18, "0.94 0.96 0.99");
        DrawText(sb, "F2", 8.5, "0.06 0.09 0.16", 48, y - 12, string.Format(CultureInfo.InvariantCulture, "FICHA EJECUTIVA: {0} ({1} min - Personal: {2}) - Dictamen: {3}", Sanitize(svc.Name), svc.DuracionMinutos, Sanitize(svc.StaffRole), ResolveExecutiveSemaphoreText(sem.Semaphore)));
        var costText = string.Format(CultureInfo.InvariantCulture, "Costes -> Productos con merma: {0:0.00} EUR | Personal imputado: {1:0.00} EUR | Gastos Generales: {2:0.00} EUR | Escandallo Total: {3:0.00} EUR", calc.CosteProductos, calc.CostePersonal, calc.CosteGastosGenerales, calc.CosteTotal);
        DrawText(sb, "F1", 7.8, "0.20 0.25 0.33", 48, y - 28, costText);
        var objText = string.Format(CultureInfo.InvariantCulture, "Rentabilidad -> Beneficio Neto Objetivo: {0:0.00} EUR | Precio sin IVA: {1:0.00} EUR | PVP Recomendado: {2:0.00} EUR", calc.BeneficioNeto, sem.PriceExVat, calc.PvpRecomendado);
        DrawText(sb, "F1", 7.8, "0.20 0.25 0.33", 48, y - 39, objText);
        decimal realMarginPct = calc.CosteTotal > 0m ? (sem.NetProfit / calc.CosteTotal) * 100m : 0m;
        var realText = string.Format(CultureInfo.InvariantCulture, "Margen Real -> PVP Comercial fijado: {0:0.00} EUR | Beneficio Neto Real: {1:0.00} EUR ({2:0.0}%)", svc.PvpComercialFijado, sem.NetProfit, realMarginPct);
        DrawText(sb, "F2", 7.8, "0.12 0.25 0.69", 48, y - 50, realText);
        DrawLine(sb, 48, y - 55, 545, y - 55, "0.88 0.91 0.95", 0.5);
        return RenderRecipeLines(sb, tenant, svc, y - 56, cardH);
    }

    private static int RenderRecipeLines(StringBuilder sb, TenantCatalogDto tenant, EscandalloServiceRecord svc, int curY, int cardH) {
        for (int r = 0; r < svc.Recipe.Count; r++) {
            var item = svc.Recipe[r];
            var prod = tenant.Products.FirstOrDefault(p => p.Name == item.ProductName);
            decimal cost = prod is null ? 0m : Math.Round((prod.PrecioCompra / prod.CantidadMl) * item.CantidadServicioMl * (1m + tenant.Parameters.MermaPct), 2, MidpointRounding.AwayFromZero);
            var recipeText = string.Format(CultureInfo.InvariantCulture, "  Receta -> {0}: {1:0.##} ml (Coste imputado con merma: {2:0.00} EUR)", Sanitize(item.ProductName), item.CantidadServicioMl, cost);
            DrawText(sb, "F1", 7.5, "0.27 0.33 0.42", 52, curY - (r * 12) - 8, recipeText);
        }
        return curY + 56 - cardH - 10;
    }

    private static void RenderPageFooter(StringBuilder sb, int pageNum, int totalPages) {
        DrawLine(sb, 36, 40, 559, 40, "0.80 0.84 0.88", 0.5);
        DrawText(sb, "F1", 7.5, "0.40 0.45 0.53", 36, 28, "Escandallo SaaS - Plataforma de Gestion de Rentabilidad para Centros de Estetica");
        DrawText(sb, "F2", 7.5, "0.40 0.45 0.53", 440, 28, $"Documento Confidencial | Pag. {pageNum} de {totalPages}");
    }

    private static void DrawSemaphoreBadge(StringBuilder sb, int x, int y, int w, int h, string sem) {
        (string fill, string stroke, string textCol) = sem == "OPTIMO" ? ("0.86 0.99 0.91", "0.13 0.65 0.33", "0.08 0.33 0.18") : (sem == "ALERTA" ? ("1.0 0.98 0.76", "0.79 0.54 0.02", "0.44 0.25 0.07") : ("1.0 0.89 0.89", "0.86 0.15 0.15", "0.50 0.11 0.11"));
        DrawFilledBox(sb, x, y, w, h, fill, stroke);
        DrawText(sb, "F2", 7, textCol, x + 4, y + 4, ResolveExecutiveSemaphoreText(sem));
    }

    private static void DrawFillRect(StringBuilder sb, int x, int y, int w, int h, string rgb) =>
        sb.Append(CultureInfo.InvariantCulture, $"{rgb} rg\n{x} {y} {w} {h} re f\n");

    private static void DrawStrokeRect(StringBuilder sb, int x, int y, int w, int h, string rgb) =>
        sb.Append(CultureInfo.InvariantCulture, $"{rgb} RG\n0.75 w\n{x} {y} {w} {h} re S\n");

    private static void DrawFilledBox(StringBuilder sb, int x, int y, int w, int h, string fillRgb, string strokeRgb) =>
        sb.Append(CultureInfo.InvariantCulture, $"{fillRgb} rg\n{strokeRgb} RG\n0.75 w\n{x} {y} {w} {h} re B\n");

    private static void DrawLine(StringBuilder sb, int x1, int y1, int x2, int y2, string rgb, double width) =>
        sb.Append(CultureInfo.InvariantCulture, $"{rgb} RG\n{width} w\n{x1} {y1} m {x2} {y2} l S\n");

    private static void DrawText(StringBuilder sb, string font, double size, string rgb, int x, int y, string text) =>
        sb.Append(CultureInfo.InvariantCulture, $"BT\n/{font} {size} Tf\n{rgb} rg\n1 0 0 1 {x} {y} Tm\n({text}) Tj\nET\n");

    private static byte[] EncodePdfDocument(string streamContent) {
        var bytes = Encoding.ASCII.GetBytes(streamContent);
        var pdf = $"%PDF-1.4\n1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources<< /Font<< /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>endobj\n4 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n5 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>endobj\n6 0 obj<< /Length {bytes.Length} >>stream\n{streamContent}endstream\nendobj\ntrailer<< /Root 1 0 R >>\n%%EOF";
        return Encoding.ASCII.GetBytes(pdf);
    }

    private static string ResolveExecutiveSemaphoreText(string semaphore) =>
        semaphore == "OPTIMO" ? "Rentabilidad optima" : (semaphore == "ALERTA" ? "Margen ajustado" : "Atencion: perdida");

    private static string Sanitize(string s) => s.Replace("(", "[").Replace(")", "]");
}
