using System;
using System.Collections.Generic;
using System.Linq;

namespace Escandallo.Core;

public static class MultiTenantSeedCatalog
{
    public static TenantCatalogDto CreateDefaultCenter(string centerId, string centerName, string ownerId = "cliente-a@estetica.es")
        => new(centerId, centerName, ownerId, "FREE", new GlobalParameters(),
            new Dictionary<string, decimal> { ["gota"] = 0.05m, ["pulsacion"] = 2.0m, ["cucharadita"] = 5.0m, ["ampolla_vial"] = 2.0m },
            new List<ProductInput> { new("Cleasing Milk", 200m, 12m, 2m), new("Crema Reafirmante", 200m, 14m, 2m), new("Colagen Mask", 100m, 18m, 4m), new("Lifting Peeling", 100m, 15m, 1m) },
            new List<StaffRoleInput> { new("Tecnico", 1000m), new("Enfermero", 1600m), new("Doctor", 2000m) },
            new List<OverheadExpenseInput> { new("Local", 750m), new("Consumos", 250m), new("Consumibles", 250m) },
            new List<EscandalloServiceRecord> {
                new("esc-001", "Higiene Facial", 60, "Tecnico", 32.00m, false, new List<ProductRecipeLine> { new("Cleasing Milk", 2m), new("Crema Reafirmante", 2m), new("Colagen Mask", 4m) }),
                new("esc-002", "Peeling Facial", 30, "Doctor", 28.00m, false, new List<ProductRecipeLine> { new("Cleasing Milk", 2m), new("Lifting Peeling", 1m), new("Crema Reafirmante", 2m) })
            },
            new List<CostHistoryEntry>());

    public static EscandalloCalculationResult ComputeService(TenantCatalogDto t, EscandalloServiceRecord s) {
        var p = t.Parameters;
        decimal ovhRate = CostCatalogService.CalculateOverheadMinuteRate(t.Overheads.Sum(o => o.ImporteMensual), p.MinutosMensuales);
        decimal staffSalary = t.Staff.FirstOrDefault(r => r.Role == s.StaffRole)?.SueldoMensual ?? 1000m;
        decimal staffRate = CostCatalogService.CalculateUnitCost(s.StaffRole, "personal", p.MinutosMensuales, staffSalary, 0m, p.CoeficienteEmpresa).UnitCost;
        decimal prodCost = Math.Round(s.Recipe.Sum(line => { var pr = t.Products.First(x => x.Name == line.ProductName); return (pr.PrecioCompra / pr.CantidadMl) * line.CantidadServicioMl * (1m + p.MermaPct); }), 2, MidpointRounding.AwayFromZero);
        decimal ovh = Math.Round(s.DuracionMinutos * ovhRate, 2, MidpointRounding.AwayFromZero);
        decimal stf = Math.Round(s.DuracionMinutos * staffRate, 2, MidpointRounding.AwayFromZero);
        decimal tot = prodCost + stf + ovh;
        decimal pvp = Math.Round(EscandalloCalculatorService.CalculateServicePvpWithVat(s.DuracionMinutos, ovhRate, staffRate, prodCost, p.Beneficio, p.Iva), 2, MidpointRounding.AwayFromZero);
        return new EscandalloCalculationResult(prodCost, stf, ovh, tot, Math.Round(tot * p.Beneficio, 2, MidpointRounding.AwayFromZero), pvp);
    }
}
