export interface ExecutiveReportInput {
  centerName: string;
  subscriptionStatus: string;
  activeServicesCount: number;
  format: string;
  concurrentExportsInFlight: number;
}

export interface ExecutiveReportResult {
  httpStatus: number;
  errorCode?: string;
  retryAfterHeader?: string;
  exportedRowsCount?: number;
  includedColumns?: string[];
}

export interface ExcelArchiveInspectionInput {
  compressedBytes: number;
  uncompressedBytes: number;
  containsExternalDtdOrEntity: boolean;
  rowCount: number;
}

export interface ExcelArchiveInspectionResult {
  httpStatus: number;
  importedRows?: number;
  auditSeverity?: 'SECURITY' | 'WARN' | 'INFO';
}

export interface SanitizedCellResult {
  sanitizedValue: string;
  status: 'LIMPIO' | 'SANITIZADO';
}

const EXECUTIVE_REPORT_COLUMNS: string[] = [
  'Gastos Generales',
  'Coste Persona',
  'Coste de Productos',
  'Escandallo Total',
  'Beneficio Neto',
  'Precio sin IVA',
  'PVP Calculado',
  'PVP Comercial Fijado',
  'Semaforo de Margen',
];

const FORMULA_INJECTION_REGEX = /^[=+\-@\t\r]/;

function isExceedingZipLimits(input: ExcelArchiveInspectionInput): boolean {
  const compressionRatio = input.uncompressedBytes / Math.max(1, input.compressedBytes);
  return (
    input.compressedBytes > 2_000_000 ||
    input.uncompressedBytes > 10_000_000 ||
    compressionRatio > 20
  );
}

export async function exportExecutiveReport(input: ExecutiveReportInput): Promise<ExecutiveReportResult> {
  if (input.format !== 'pdf' && input.format !== 'xlsx') {
    return { httpStatus: 400, errorCode: 'UNSUPPORTED_EXPORT_FORMAT' };
  }
  if (input.activeServicesCount <= 0) {
    return { httpStatus: 422, errorCode: 'NO_ACTIVE_SERVICES_TO_EXPORT' };
  }
  if (input.subscriptionStatus !== 'active' && input.activeServicesCount > 2) {
    return { httpStatus: 402, errorCode: 'QUOTA_EXCEEDED_EXPORT_LOCK' };
  }
  if (input.concurrentExportsInFlight >= 2) {
    return { httpStatus: 429, retryAfterHeader: '5' };
  }
  return {
    httpStatus: 200,
    exportedRowsCount: input.activeServicesCount,
    includedColumns: [...EXECUTIVE_REPORT_COLUMNS],
  };
}

export async function inspectAndParseExcelCatalog(
  input: ExcelArchiveInspectionInput,
): Promise<ExcelArchiveInspectionResult> {
  if (input.containsExternalDtdOrEntity || isExceedingZipLimits(input) || input.rowCount > 5000) {
    return { httpStatus: 422, auditSeverity: 'SECURITY' };
  }
  return { httpStatus: 200, importedRows: input.rowCount };
}

export function sanitizeSpreadsheetCell(rawValue: string): SanitizedCellResult {
  if (FORMULA_INJECTION_REGEX.test(rawValue)) {
    return { sanitizedValue: `'${rawValue}`, status: 'SANITIZADO' };
  }
  return { sanitizedValue: rawValue, status: 'LIMPIO' };
}
