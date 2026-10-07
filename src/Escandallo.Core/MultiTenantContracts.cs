using System.Collections.Generic;

namespace Escandallo.Core;

public record GlobalParameters(decimal Iva = 0.21m, decimal Beneficio = 0.25m, decimal HorasDia = 8m, decimal DiasMes = 22m, int MinutosMensuales = 10560, decimal CoeficienteEmpresa = 1.35m, decimal MermaPct = 0.03m);
public record ProductInput(string Name, decimal CantidadMl, decimal PrecioCompra, decimal CantidadPorServicioMl);
public record StaffRoleInput(string Role, decimal SueldoMensual);
public record OverheadExpenseInput(string Concepto, decimal ImporteMensual);
public record ProductRecipeLine(string ProductName, decimal CantidadServicioMl);
public record CostHistoryEntry(string CostItemId, decimal PreviousValue, decimal NewValue, string ChangedAtIso);
public record EscandalloCalculationResult(decimal CosteProductos, decimal CostePersonal, decimal CosteGastosGenerales, decimal CosteTotal, decimal BeneficioNeto, decimal PvpRecomendado);
public record EscandalloServiceRecord(string Id, string Name, int DuracionMinutos, string StaffRole, decimal PvpComercialFijado, bool Archived, List<ProductRecipeLine> Recipe);
public record TenantCatalogDto(string CenterId, string CenterName, string OwnerId, string SubscriptionPlan, GlobalParameters Parameters, Dictionary<string, decimal> DoseEquivalencesMl, List<ProductInput> Products, List<StaffRoleInput> Staff, List<OverheadExpenseInput> Overheads, List<EscandalloServiceRecord> Escandallos, List<CostHistoryEntry> CostHistory);
