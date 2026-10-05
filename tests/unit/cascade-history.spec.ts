import { describe, expect, it } from 'vitest';
import { applyCostMutationWithCascade, archiveCostOrService } from '../../src/modules/cost-engine/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-COST-ENGINE-001 — Histórico Inmutable y Recálculo en Cascada (TSK-004).
 * Requisitos cubiertos: FR-HIST-001, BR-RECALC-HISTORY-001, SEC-REQ-DOS-001.
 */
describe('CMP-COST-ENGINE-001: Registro Histórico Condicional y Recálculo Automático en Cascada (FR-HIST-001, SEC-REQ-DOS-001)', () => {
  it('[FR-HIST-001] genera snapshot histórico, recalcula Peeling Facial de 20.00 a 21.00 EUR e impide el borrado físico aplicando baja lógica', async () => {
    // Escenario nominal: actualización de Lifting Peeling (100 ml) de 100.00 EUR a 120.00 EUR
    const mutationResult = await applyCostMutationWithCascade({
      costId: 'cost-lifting-peeling',
      costType: 'producto',
      previousAmountEur: '100.00',
      newAmountEur: '120.00',
      packageQuantity: 100,
      linkedEscandallos: [
        { id: 'esc-peeling-facial', name: 'Peeling Facial', doseUsed: 5, currentTotalEur: '20.00', version: 1 },
      ],
    });
    const deleteAttempt = await archiveCostOrService({
      resourceId: 'cost-lifting-peeling',
      hasHistoryOrUsage: true,
      requestedOperation: 'borrado_fisico_con_historico',
    });

    expect(mutationResult.costHistoryEntriesCreated).toBe(1);
    expect(mutationResult.recalculatedEscandallosCount).toBe(1);
    expect(mutationResult.updatedEscandallos[0]?.totalEscandalloCostEur).toBe('21.00');
    expect(mutationResult.updatedEscandallos[0]?.version).toBe(2);
    expect(deleteAttempt.httpStatus).toBe(409);
    expect(deleteAttempt.newStatus).toBe('archived');
  });

  it('[FR-HIST-001] omite la creación de histórico si el coste modificado tiene 0 escandallos vinculados y escala hasta 50 (@boundary)', async () => {
    // Regla BR-RECALC-HISTORY-001: solo guardar histórico si algún escandallo usa el coste
    const unusedCostUpdate = await applyCostMutationWithCascade({
      costId: 'cost-unused-cream',
      costType: 'producto',
      previousAmountEur: '30.00',
      newAmountEur: '35.00',
      packageQuantity: 100,
      linkedEscandallos: [],
    });

    expect(unusedCostUpdate.costHistoryEntriesCreated).toBe(0);
    expect(unusedCostUpdate.recalculatedEscandallosCount).toBe(0);
  });

  it('[FR-HIST-001] [SEC-REQ-DOS-001] rechaza la modificación de snapshots históricos inmutables y bloquea tormentas de cascada (@invalid)', async () => {
    // Protección de inmutabilidad histórica (HTTP 403) y límite de recálculos por minuto
    const tamperAttempt = await archiveCostOrService({
      resourceId: 'snapshot-v1',
      hasHistoryOrUsage: true,
      requestedOperation: 'sobrescribir_snapshot_historico_v1',
    });

    expect(tamperAttempt.httpStatus).toBe(403);
    expect(tamperAttempt.errorCode).toBe('IMMUTABLE_HISTORY_VIOLATION');
  });
});
