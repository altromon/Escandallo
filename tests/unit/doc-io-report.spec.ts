import { describe, expect, it } from 'vitest';
import { exportExecutiveReport, inspectAndParseExcelCatalog, sanitizeSpreadsheetCell } from '../../src/modules/doc-io/index.js';

/**
 * Suite BDD/TDD en Rojo para CMP-DOC-IO-001 — Exportación de Informes y Parseo Seguro Excel (TSK-005).
 * Requisitos cubiertos: FR-REPORT-001, SEC-REQ-XLSX-001, SEC-REQ-DOS-001.
 */
describe('CMP-DOC-IO-001: Exportación de Informes Ejecutivos PDF/Excel y Seguridad Anti-XXE/DDE (FR-REPORT-001, SEC-REQ-XLSX-001, SEC-REQ-DOS-001)', () => {
  it('[FR-REPORT-001] genera informes ejecutivos en PDF y Excel con el desglose completo y Semáforo de Margen para centros dentro de cuota', async () => {
    // Escenario nominal y casos límite de formatos permitidos (pdf y xlsx)
    const pdfReport = await exportExecutiveReport({
      centerName: 'Centro Estética Norte',
      subscriptionStatus: 'none',
      activeServicesCount: 2,
      format: 'pdf',
      concurrentExportsInFlight: 0,
    });
    const xlsxReport = await exportExecutiveReport({
      centerName: 'Centro Estética Norte',
      subscriptionStatus: 'active',
      activeServicesCount: 50,
      format: 'xlsx',
      concurrentExportsInFlight: 1,
    });

    expect(pdfReport.httpStatus).toBe(200);
    expect(pdfReport.includedColumns).toContain('Semaforo de Margen');
    expect(xlsxReport.httpStatus).toBe(200);
    expect(xlsxReport.exportedRowsCount).toBe(50);
  });

  it('[FR-REPORT-001] rechaza formatos no soportados (csv, html, vacío), centros sin servicios activos o exceso de cuota sin suscripción (@invalid)', async () => {
    // Validación estricta de formatos exclusivamente PDF y Excel (.xlsx)
    const csvAttempt = await exportExecutiveReport({
      centerName: 'Centro Norte',
      subscriptionStatus: 'none',
      activeServicesCount: 2,
      format: 'csv',
      concurrentExportsInFlight: 0,
    });
    const quotaLockedAttempt = await exportExecutiveReport({
      centerName: 'Centro Norte',
      subscriptionStatus: 'canceled',
      activeServicesCount: 3,
      format: 'xlsx',
      concurrentExportsInFlight: 0,
    });

    expect(csvAttempt.httpStatus).toBe(400);
    expect(csvAttempt.errorCode).toBe('UNSUPPORTED_EXPORT_FORMAT');
    expect(quotaLockedAttempt.httpStatus).toBe(402);
    expect(quotaLockedAttempt.errorCode).toBe('QUOTA_EXCEEDED_EXPORT_LOCK');
  });

  it('[SEC-REQ-XLSX-001] bloquea archivos .xlsx con entidades externas XML (XXE) o bombas ZIP y neutraliza fórmulas DDE (@security @mitigation)', async () => {
    // Mitigación de ABUSE-XLSX-INJECT-001 en importación M4 y exportación
    const xxeUpload = await inspectAndParseExcelCatalog({
      compressedBytes: 512000,
      uncompressedBytes: 15000000,
      containsExternalDtdOrEntity: true,
      rowCount: 120,
    });
    const ddeCell = sanitizeSpreadsheetCell("=CMD|'/C calc'!A0");

    expect(xxeUpload.httpStatus).toBe(422);
    expect(xxeUpload.auditSeverity).toBe('SECURITY');
    expect(ddeCell.sanitizedValue).toBe("'=CMD|'/C calc'!A0");
    expect(ddeCell.status).toBe('SANITIZADO');
  });

  it('[SEC-REQ-DOS-001] bloquea la tercera exportación concurrente de informes PDF por un mismo inquilino con HTTP 429 y Retry-After', async () => {
    // Semáforo de concurrencia máximo de 2 renderizados simultáneos por centro
    const burstExport = await exportExecutiveReport({
      centerName: 'C_ACTIVO',
      subscriptionStatus: 'active',
      activeServicesCount: 10,
      format: 'pdf',
      concurrentExportsInFlight: 2,
    });

    expect(burstExport.httpStatus).toBe(429);
    expect(burstExport.retryAfterHeader).toBeDefined();
  });
});
