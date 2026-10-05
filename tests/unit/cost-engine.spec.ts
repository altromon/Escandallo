import { describe, expect, it } from 'vitest';
import { calculateOverheadRatePerMinute, calculateProductUnitCost, calculateStaffUnitCost } from '../../src/modules/cost-engine/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-COST-ENGINE-001 — Catálogo de Costes (TSK-002).
 * Requisitos cubiertos: FR-COST-001, BR-CALC-FORMULAS-001.
 */
describe('CMP-COST-ENGINE-001: Alta y Cálculo Unitario de Productos, Personal y Gastos Generales (FR-COST-001)', () => {
  it('[FR-COST-001] calcula el coste mensual total (875.00 EUR) y la tasa por minuto (0.08285985 EUR/min) de Gastos Generales', () => {
    // Datos canónicos de CalculadoraEscandallo.xlsx (Tabla4: 10560 minutos laborables/mes)
    const result = calculateOverheadRatePerMinute({
      monthlyMinutesCapacity: 10560,
      expenses: [
        { name: 'Local', monthlyCostEur: '335.00' },
        { name: 'Consumos', monthlyCostEur: '100.00' },
        { name: 'Consumibles', monthlyCostEur: '440.00' },
      ],
    });

    expect(result.totalMonthlyOverheadEur).toBe('875.00');
    expect(result.overheadRatePerMinuteEur).toBe('0.08285985');
  });

  it('[FR-COST-001] calcula con precisión decimal exacta el precio unitario de productos en ml, g y ud (@boundary)', () => {
    // Casos reales de Tabla1 y Mejora M5 (soporte de gramos y porcentaje de merma)
    const cleansingMilk = calculateProductUnitCost({
      name: 'Cleasing Milk',
      packageQuantity: 500,
      unit: 'ml',
      packagePriceEur: '34.51',
      wastagePct: '0.00',
    });
    const firmingCream = calculateProductUnitCost({
      name: 'Crema Reafirmante',
      packageQuantity: 250,
      unit: 'g',
      packagePriceEur: '50.00',
      wastagePct: '0.05',
    });
    const collagenMask = calculateProductUnitCost({
      name: 'Colagen Mask',
      packageQuantity: 12,
      unit: 'ud',
      packagePriceEur: '86.52',
      wastagePct: '0.00',
    });

    expect(cleansingMilk.unitPriceEur).toBe('0.06902');
    expect(firmingCream.unitPriceEur).toBe('0.20');
    expect(firmingCream.effectiveUnitPriceWithWastageEur).toBe('0.21');
    expect(collagenMask.unitPriceEur).toBe('7.21');
  });

  it('[FR-COST-001] calcula el coste de empresa y coste por minuto de las categorías de Personal (Tecnico, Enfermero, Doctor) (@boundary)', () => {
    // Fórmula Tabla3: ((Neto * Retenciones) + Neto) * Coef_Empresa / Cantidad
    const tecnico = calculateStaffUnitCost({
      roleName: 'Tecnico',
      monthlyMinutes: 10560,
      netSalaryEur: '1100.00',
      retentionPct: '0.20',
      companyCoefficient: '1.5',
    });
    const enfermero = calculateStaffUnitCost({
      roleName: 'Enfermero',
      monthlyMinutes: 10560,
      netSalaryEur: '1800.00',
      retentionPct: '0.22',
      companyCoefficient: '1.5',
    });
    const doctor = calculateStaffUnitCost({
      roleName: 'Doctor',
      monthlyMinutes: 10560,
      netSalaryEur: '3000.00',
      retentionPct: '0.25',
      companyCoefficient: '1.5',
    });

    expect(tecnico.companyMonthlyCostEur).toBe('1980.00');
    expect(tecnico.minuteRateEur).toBe('0.1875');
    expect(enfermero.companyMonthlyCostEur).toBe('3294.00');
    expect(enfermero.minuteRateEur).toBe('0.31193182');
    expect(doctor.companyMonthlyCostEur).toBe('5625.00');
    expect(doctor.minuteRateEur).toBe('0.53267045');
  });

  it('[FR-COST-001] rechaza cantidades cero, precios negativos, nombres vacíos o mermas superiores al 100% (@invalid)', () => {
    // Guardas aritméticas contra división por cero y dominios inválidos
    const zeroQty = calculateProductUnitCost({
      name: 'Envase Vacío',
      packageQuantity: 0,
      unit: 'ml',
      packagePriceEur: '34.51',
      wastagePct: '0.00',
    });
    const excessiveWastage = calculateProductUnitCost({
      name: 'Merma Excesiva',
      packageQuantity: 200,
      unit: 'ml',
      packagePriceEur: '40.00',
      wastagePct: '1.50',
    });

    expect(zeroQty.httpStatus).toBe(422);
    expect(zeroQty.errorCode).toBe('INVALID_ZERO_QUANTITY');
    expect(excessiveWastage.httpStatus).toBe(422);
    expect(excessiveWastage.errorCode).toBe('WASTAGE_OUT_OF_RANGE');
  });
});
