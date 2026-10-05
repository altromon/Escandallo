using Escandallo.Core.Modules.CostEngine;
using Xunit;

namespace Escandallo.UnitTests;

/// <summary>
/// Suite BDD/TDD en C# xUnit para CMP-COST-ENGINE-001 — Catálogo de Costes (TSK-002).
/// Requisitos cubiertos: FR-COST-001, BR-CALC-FORMULAS-001.
/// </summary>
public sealed class CostEngineTests
{
    [Fact]
    public void FrCost001_CalculatesExactOverheadRatePerMinuteFromExcelTabla1() {
        // Paridad matemática exacta con Tabla1 del Excel (875.00 / 10560 = 0.08285985 EUR/min)
        var expenses = new[]
        {
            new OverheadExpenseItem("Alquiler", "335.00"),
            new OverheadExpenseItem("Luz", "100.00"),
            new OverheadExpenseItem("Autonomo", "440.00")
        };
        var overhead = CostCatalogCalculator.CalculateOverheadRatePerMinute(new OverheadRateInput(10560, expenses));
        Assert.Equal("875.00", overhead.TotalMonthlyOverheadEur);
        Assert.Equal("0.08285985", overhead.OverheadRatePerMinuteEur);
    }

    [Fact]
    public void FrCost001_CalculatesProductUnitCostAndEffectiveWastageM5() {
        // Paridad matemática con Tabla2 y mejora M5 (% Merma configurable en ml, g y ud)
        var cleasingMilk = CostCatalogCalculator.CalculateProductUnitCost(
            new ProductUnitCostInput("Cleasing Milk", 500, "ml", "34.51", "0.00"));
        var glycolicWithWastage = CostCatalogCalculator.CalculateProductUnitCost(
            new ProductUnitCostInput("Acido Glicolico", 250, "g", "50.00", "0.05"));
        var colagenMask = CostCatalogCalculator.CalculateProductUnitCost(
            new ProductUnitCostInput("Colagen Mask", 12, "ud", "86.52", "0.00"));
        Assert.Equal("0.06902", cleasingMilk.UnitPriceEur);
        Assert.Equal("0.20", glycolicWithWastage.UnitPriceEur);
        Assert.Equal("0.21", glycolicWithWastage.EffectiveUnitPriceWithWastageEur);
        Assert.Equal("7.21", colagenMask.UnitPriceEur);
    }

    [Fact]
    public void FrCost001_CalculatesStaffMonthlyAndPerMinuteCostForAllRoles() {
        // Paridad matemática con Tabla3 y Tabla4 (Tecnico, Enfermero, Doctor)
        var tecnico = CostCatalogCalculator.CalculateStaffUnitCost(
            new StaffUnitCostInput("Tecnico", 10560, "1100.00", "0.20", "1.5"));
        var enfermero = CostCatalogCalculator.CalculateStaffUnitCost(
            new StaffUnitCostInput("Enfermero", 10560, "1800.00", "0.22", "1.5"));
        var doctor = CostCatalogCalculator.CalculateStaffUnitCost(
            new StaffUnitCostInput("Doctor", 10560, "3000.00", "0.25", "1.5"));
        Assert.Equal("1980.00", tecnico.CompanyMonthlyCostEur);
        Assert.Equal("0.1875", tecnico.MinuteRateEur);
        Assert.Equal("0.31193182", enfermero.MinuteRateEur);
        Assert.Equal("0.53267045", doctor.MinuteRateEur);
    }

    [Fact]
    public void FrCost001_RejectsZeroDivisorsNegativePricesAndOutOfRangeWastage() {
        // Validación de guardias aritméticas (@boundary @invalid) con HTTP 422
        var zeroQuantity = CostCatalogCalculator.CalculateProductUnitCost(
            new ProductUnitCostInput("Envase Vacio", 0, "ml", "25.00", "0.00"));
        var negativePrice = CostCatalogCalculator.CalculateProductUnitCost(
            new ProductUnitCostInput("Crema Negativa", 100, "ml", "-15.00", "0.00"));
        var excessiveWastage = CostCatalogCalculator.CalculateProductUnitCost(
            new ProductUnitCostInput("Merma Excesiva", 100, "ml", "20.00", "1.50"));
        var zeroMinutesStaff = CostCatalogCalculator.CalculateStaffUnitCost(
            new StaffUnitCostInput("Tecnico", 0, "1100.00", "0.20", "1.5"));
        Assert.Equal(422, zeroQuantity.HttpStatus);
        Assert.Equal(422, negativePrice.HttpStatus);
        Assert.Equal(422, excessiveWastage.HttpStatus);
        Assert.Equal(422, zeroMinutesStaff.HttpStatus);
    }
}
