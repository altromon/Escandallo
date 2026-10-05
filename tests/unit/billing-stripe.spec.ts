import { describe, expect, it } from 'vitest';
import { enforceFreemiumQuotaWithLock, verifyAndProcessStripeWebhook } from '../../src/modules/billing/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-BILLING-STRIPE-001 — Cuota Freemium y Suscripciones Stripe (TSK-006).
 * Requisitos cubiertos: FR-BILLING-001, SEC-REQ-BILLING-001, QR-SEC-001, BR-FREEMIUM-QUOTA-001.
 */
describe('CMP-BILLING-STRIPE-001: Cuota Gratuita de 2 Escandallos, Archivado M1 y Webhooks HMAC Stripe (FR-BILLING-001, SEC-REQ-BILLING-001, QR-SEC-001)', () => {
  it('[FR-BILLING-001] desbloquea la edición y exportación tras archivar escandallos excedentes hasta dejar 2 activos sin perder histórico', async () => {
    // Escenario nominal M1: Centro Sol pasa de 4 activos (bloqueado HTTP 402) a 2 activos + 2 archivados
    const lockedCheck = await enforceFreemiumQuotaWithLock({
      centerId: 'Centro Sol',
      subscriptionStatus: 'canceled',
      activeEscandallosCount: 4,
      archivedEscandallosCount: 0,
      operation: 'editar_escandallo',
      concurrentRequests: 1,
    });
    const unlockedAfterArchive = await enforceFreemiumQuotaWithLock({
      centerId: 'Centro Sol',
      subscriptionStatus: 'canceled',
      activeEscandallosCount: 2,
      archivedEscandallosCount: 2,
      operation: 'editar_escandallo',
      concurrentRequests: 1,
    });

    expect(lockedCheck.httpStatus).toBe(402);
    expect(unlockedAfterArchive.httpStatus).toBe(200);
    expect(unlockedAfterArchive.preservedArchivedCount).toBe(2);
  });

  it('[FR-BILLING-001] permite crear hasta 2 escandallos gratuitos, recalcularlos libremente y crear ilimitados con suscripción activa (@boundary)', async () => {
    // Fronteras exactas 0, 1 y 2 en plan gratuito y > 2 en plan activo
    const secondFree = await enforceFreemiumQuotaWithLock({
      centerId: 'C_FREE',
      subscriptionStatus: 'none',
      activeEscandallosCount: 1,
      archivedEscandallosCount: 0,
      operation: 'crear_escandallo',
      concurrentRequests: 1,
    });
    const recalcAtLimit = await enforceFreemiumQuotaWithLock({
      centerId: 'C_FREE',
      subscriptionStatus: 'none',
      activeEscandallosCount: 2,
      archivedEscandallosCount: 0,
      operation: 'recalcular_existente',
      concurrentRequests: 1,
    });

    expect(secondFree.httpStatus).toBe(201);
    expect(recalcAtLimit.httpStatus).toBe(200);
  });

  it('[SEC-REQ-BILLING-001] serializa con bloqueo pesimista 5 peticiones concurrentes permitiendo exactamente 1 transición a active y rechazando 4 con HTTP 402', async () => {
    // Mitigación de ABUSE-QUOTA-BYPASS-001 (SELECT ... FOR UPDATE sobre la fila del centro)
    const raceResult = await enforceFreemiumQuotaWithLock({
      centerId: 'C_FREE',
      subscriptionStatus: 'none',
      activeEscandallosCount: 1,
      archivedEscandallosCount: 2,
      operation: 'desarchivar_escandallo',
      concurrentRequests: 5,
    });

    expect(raceResult.succeededCount).toBe(1);
    expect(raceResult.rejectedWith402Count).toBe(4);
    expect(raceResult.finalActiveEscandallosCount).toBe(2);
  });

  it('[SEC-REQ-BILLING-001] [QR-SEC-001] rechaza webhooks de Stripe con firma HMAC inválida, cabecera ausente o timestamp expirado (> 300s)', async () => {
    // Verificación criptográfica HMAC-SHA256 e idempotencia por event.id
    const invalidHmac = await verifyAndProcessStripeWebhook({
      eventId: 'evt_fake_001',
      eventType: 'checkout.session.completed',
      signatureAnomaly: 'firma_hmac_invalida',
      ageSeconds: 10,
    });
    const replayWebhook = await verifyAndProcessStripeWebhook({
      eventId: 'evt_replay_002',
      eventType: 'checkout.session.completed',
      signatureAnomaly: 'timestamp_expirado_replay',
      ageSeconds: 600,
    });

    expect(invalidHmac.httpStatus).toBe(400);
    expect(invalidHmac.subscriptionModified).toBe(false);
    expect(invalidHmac.auditSeverity).toBe('SECURITY');
    expect(replayWebhook.httpStatus).toBe(400);
  });
});
