import { describe, expect, it } from 'vitest';
import { authorizeCrossCenterCopy, checkTenantRateLimit, verifySessionAndRls } from '../../src/modules/iam/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-IAM-TENANT-001 (TSK-001).
 * Requisitos cubiertos: FR-TENANT-001, SEC-REQ-TENANT-001, QR-SEC-001, SEC-REQ-DOS-001.
 */
describe('CMP-IAM-TENANT-001: Aislamiento Multi-Tenant RLS, Anti-IDOR y Rate Limiting (FR-TENANT-001, SEC-REQ-TENANT-001, QR-SEC-001, SEC-REQ-DOS-001)', () => {
  it('[FR-TENANT-001] permite copiar el catálogo de productos entre centros del mismo propietario manteniendo aislados los centros ajenos', async () => {
    // Escenario nominal M4: cliente-a copia entre Centro Madrid y Centro Valencia
    const result = await authorizeCrossCenterCopy({
      authenticatedUserEmail: 'cliente-a@estetica.es',
      sourceCenterId: 'Centro Madrid',
      targetCenterId: 'Centro Valencia',
      ownerByCenter: {
        'Centro Madrid': 'cliente-a@estetica.es',
        'Centro Valencia': 'cliente-a@estetica.es',
        'Centro Sevilla': 'cliente-b@estetica.es',
      },
      catalogItemsCount: 25,
    });

    expect(result.httpStatus).toBe(200);
    expect(result.copiedItems).toBe(25);
    expect(result.isolatedForeignCenters).toContain('Centro Sevilla');
  });

  it('[FR-TENANT-001] valida los límites de volumen (0, 1 y 5000 ítems) al consultar o clonar catálogos propios (@boundary)', async () => {
    // Casos límite de titularidad y volumen máximo de 5000 filas por catálogo
    const emptyCheck = await authorizeCrossCenterCopy({
      authenticatedUserEmail: 'cliente-a@estetica.es',
      sourceCenterId: 'Centro Madrid',
      targetCenterId: 'Centro Madrid',
      ownerByCenter: { 'Centro Madrid': 'cliente-a@estetica.es' },
      catalogItemsCount: 0,
    });
    const maxCheck = await authorizeCrossCenterCopy({
      authenticatedUserEmail: 'cliente-a@estetica.es',
      sourceCenterId: 'Centro Madrid',
      targetCenterId: 'Centro Valencia',
      ownerByCenter: {
        'Centro Madrid': 'cliente-a@estetica.es',
        'Centro Valencia': 'cliente-a@estetica.es',
      },
      catalogItemsCount: 5000,
    });

    expect(emptyCheck.httpStatus).toBe(200);
    expect(emptyCheck.copiedItems).toBe(0);
    expect(maxCheck.httpStatus).toBe(200);
    expect(maxCheck.copiedItems).toBe(5000);
  });

  it('[FR-TENANT-001] [SEC-REQ-TENANT-001] rechaza copia cruzada desde un centro ajeno con HTTP 403 y emite alerta SECURITY (@invalid @security)', async () => {
    // Mitigación de ABUSE-IDOR-TENANT-001 en clonado M4 cross-center
    const attackResult = await authorizeCrossCenterCopy({
      authenticatedUserEmail: 'U_ATACANTE',
      sourceCenterId: 'C_AJENO',
      targetCenterId: 'C_PROPIO',
      ownerByCenter: {
        C_PROPIO: 'U_ATACANTE',
        C_AJENO: 'U_VICTIMA',
      },
      catalogItemsCount: 25,
    });

    expect(attackResult.httpStatus).toBe(403);
    expect(attackResult.copiedItems).toBe(0);
    expect(attackResult.auditEvent?.severity).toBe('SECURITY');
    expect(attackResult.auditEvent?.reason).toBe('CROSS_TENANT_IDOR_ATTEMPT');
  });

  it('[SEC-REQ-TENANT-001] [QR-SEC-001] bloquea accesos IDOR sobre costes e informes de otro inquilino sin filtrar metadatos', async () => {
    // Verificación de predicado RLS y respuesta opaca 403/404
    const idorRead = await verifySessionAndRls({
      authenticatedUserId: 'U_ATACANTE',
      activeCenterId: '00000000-0000-4000-8000-000000000001',
      targetCenterId: '00000000-0000-4000-8000-000000000099',
      operation: 'GET /costs',
    });
    const invalidUuid = await verifySessionAndRls({
      authenticatedUserId: 'U_ATACANTE',
      activeCenterId: '00000000-0000-4000-8000-000000000001',
      targetCenterId: 'id-no-uuid-invalido',
      operation: 'GET /costs',
    });

    expect(idorRead.httpStatus).toBe(403);
    expect(idorRead.leakedTenantData).toBeUndefined();
    expect(idorRead.auditSeverity).toBe('SECURITY');
    expect(invalidUuid.httpStatus).toBe(400);
  });

  it('[SEC-REQ-DOS-001] [QR-SEC-001] aplica rate limiting ante ráfagas de fuerza bruta o mutaciones abusivas por inquilino', async () => {
    // Mitigación de ABUSE-DOS-CASCADE-001 (máximo 30 mutaciones con cascada por minuto)
    const burstResult = await checkTenantRateLimit({
      tenantId: 'C_ACTIVO',
      operation: 'actualizacion_coste_con_cascada',
      requestsInWindow: 45,
      maxAllowedPerMinute: 30,
    });

    expect(burstResult.httpStatus).toBe(429);
    expect(burstResult.retryAfterSeconds).toBeGreaterThan(0);
    expect(burstResult.logSeverity).toBe('WARN');
  });
});
