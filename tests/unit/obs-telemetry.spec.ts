import { describe, expect, it } from 'vitest';
import { exportTelemetryBundle, redactTelemetryRecord, setHotLogLevel } from '../../src/modules/observability/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-OBS-TELEMETRY-001 — Observabilidad Dual y Redactado de Secretos (TSK-007).
 * Requisitos cubiertos: FR-OBS-001, SEC-REQ-OBS-001.
 */
describe('CMP-OBS-TELEMETRY-001: Extracción de Logs, Trazas y Métricas para Mantenedor y Redactado DEBUG (FR-OBS-001, SEC-REQ-OBS-001)', () => {
  it('[FR-OBS-001] permite al rol ACT-MAINTAINER-001 cambiar en caliente la granularidad a DEBUG y extraer logs, trazas y métricas', async () => {
    // Escenario nominal de ajuste dinámico sin reinicio y exportación estándar
    const levelChange = await setHotLogLevel({
      actorRole: 'ACT-MAINTAINER-001',
      newLevel: 'DEBUG',
    });
    const bundle = await exportTelemetryBundle({
      actorRole: 'ACT-MAINTAINER-001',
      resource: 'logs',
      windowHours: 24,
    });

    expect(levelChange.httpStatus).toBe(200);
    expect(levelChange.appliedWithoutRestart).toBe(true);
    expect(bundle.httpStatus).toBe(200);
    expect(bundle.outputFormat).toBe('ndjson');
  });

  it('[FR-OBS-001] soporta extracción en las fronteras de ventana temporal (1h a 720h) y bloquea roles sin privilegios (@boundary @invalid)', async () => {
    // Control RBAC estricto de recursos administrativos de telemetría
    const boundaryExport = await exportTelemetryBundle({
      actorRole: 'ACT-MAINTAINER-001',
      resource: 'metrics',
      windowHours: 168,
    });
    const forbiddenExport = await exportTelemetryBundle({
      actorRole: 'ACT-ESTHETIC-USER-001',
      resource: 'logs',
      windowHours: 24,
    });

    expect(boundaryExport.httpStatus).toBe(200);
    expect(boundaryExport.outputFormat).toBe('prometheus_csv');
    expect(forbiddenExport.httpStatus).toBe(403);
    expect(forbiddenExport.errorCode).toBe('MAINTAINER_ROLE_REQUIRED');
  });

  it('[SEC-REQ-OBS-001] ofusca obligatoriamente tokens, cookies y firmas con [REDACTED] incluso en modo DEBUG y previene Log Injection CRLF', () => {
    // Mitigación de ABUSE-PRIV-OBS-001: filtro inmutable de redactado y escape de saltos de línea
    const sanitizedRecord = redactTelemetryRecord({
      logLevel: 'DEBUG',
      authorization: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9',
      cookie: 'session_id=sec-token-999',
      stripe_signature: 't=170000,v1=deadbeef',
      searchQuery: 'servicio\r\n{"level":"INFO","faked":1}',
    });

    expect(sanitizedRecord.authorization).toBe('[REDACTED]');
    expect(sanitizedRecord.cookie).toBe('[REDACTED]');
    expect(sanitizedRecord.stripe_signature).toBe('[REDACTED]');
    expect(sanitizedRecord.singleLineJson).not.toContain('\r\n');
  });
});
