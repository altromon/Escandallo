import { describe, expect, it } from 'vitest';
import { calculateEscandalloService, evaluateMarginSemaphore } from '../../src/modules/cost-engine/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-COST-ENGINE-001 — Motor de Escandallos y Semáforo de Margen (TSK-003).
 * Requisitos cubiertos: FR-CALC-001, BR-CALC-FORMULAS-001.
 */
describe('CMP-COST-ENGINE-001: Cálculo de Escandallo Total, Beneficio Neto, PVP y Semáforo de Margen (FR-CALC-001)', () => {
  it('[FR-CALC-001] calcula el desglose completo del servicio Higiene Facial (60 min) con precisión decimal exacta', () => {
    // Escenario nominal extraído de Tabla5 y Tabla57 de CalculadoraEscandallo.xlsx
    const result = calculateEscandalloService({
      serviceName: 'Higiene Facial',
      durationMinutes: 60,
      overheadRatePerMinuteEur: '0.08285985',
      staffMinuteRateEur: '0.1875',
      targetProfitPct: '0.50',
      vatPct: '0.21',
      productLines: [
        { name: 'Cleasing Milk', doseQuantity: '10', unitPriceEur: '0.06902', wastagePct: '0.00' },
        { name: 'Colagen Mask', doseQuantity: '1', unitPriceEur: '7.21', wastagePct: '0.00' },
      ],
    });

    expect(result.allocatedOverheadEur).toBe('4.971591');
    expect(result.staffCostEur).toBe('11.25');
    expect(result.productsCostEur).toBe('7.9002');
    expect(result.totalEscandalloCostEur).toBe('24.121791');
    expect(result.netProfitEur).toBe('12.0608955');
    expect(result.priceWithProfitExclVatEur).toBe('36.1826865');
    expect(result.calculatedPvpInclVatEur).toBe('43.78105067');
  });

  it('[FR-CALC-001] evalúa las fronteras exactas del Semáforo de Erosión de Margen (OPTIMO, ALERTA, CRITICO) frente al PVP Comercial (@boundary)', () => {
    // Mejora M2: evaluación de beneficio real y erosión de margen en 10.00, 5.00, 0.00 y -2.00 EUR
    const optimo = evaluateMarginSemaphore({
      totalEscandalloCostEur: '20.00',
      targetProfitPct: '0.50',
      vatPct: '0.21',
      fixedCommercialPvpEur: '36.30',
    });
    const alerta = evaluateMarginSemaphore({
      totalEscandalloCostEur: '25.00',
      targetProfitPct: '0.50',
      vatPct: '0.21',
      fixedCommercialPvpEur: '36.30',
    });
    const criticoFronteraCero = evaluateMarginSemaphore({
      totalEscandalloCostEur: '30.00',
      targetProfitPct: '0.50',
      vatPct: '0.21',
      fixedCommercialPvpEur: '36.30',
    });

    expect(optimo.realNetProfitEur).toBe('10.00');
    expect(optimo.semaphoreStatus).toBe('OPTIMO');
    expect(alerta.realNetProfitEur).toBe('5.00');
    expect(alerta.semaphoreStatus).toBe('ALERTA');
    expect(criticoFronteraCero.realNetProfitEur).toBe('0.00');
    expect(criticoFronteraCero.semaphoreStatus).toBe('CRITICO');
  });

  it('[FR-CALC-001] rechaza servicios con duración cero/negativa, dosis negativas o más de 100 líneas de productos (@invalid)', () => {
    // Validación de entradas fuera de rango y cardinalidad máxima por receta
    const zeroDuration = calculateEscandalloService({
      serviceName: 'Facial Cero',
      durationMinutes: 0,
      overheadRatePerMinuteEur: '0.08285985',
      staffMinuteRateEur: '0.1875',
      targetProfitPct: '0.50',
      vatPct: '0.21',
      productLines: [],
    });
    const excessiveLines = calculateEscandalloService({
      serviceName: 'Receta Masiva',
      durationMinutes: 60,
      overheadRatePerMinuteEur: '0.08285985',
      staffMinuteRateEur: '0.1875',
      targetProfitPct: '0.50',
      vatPct: '0.21',
      productLines: Array.from({ length: 101 }, (_, idx) => ({
        name: `Producto ${idx}`,
        doseQuantity: '1',
        unitPriceEur: '0.50',
        wastagePct: '0.00',
      })),
    });

    expect(zeroDuration.httpStatus).toBe(422);
    expect(zeroDuration.errorCode).toBe('INVALID_SERVICE_DURATION');
    expect(excessiveLines.httpStatus).toBe(422);
    expect(excessiveLines.errorCode).toBe('MAX_RECIPE_LINES_EXCEEDED');
  });
});
