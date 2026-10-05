export interface OverheadExpenseItem {
  name: string;
  monthlyCostEur: string;
}

export interface OverheadRateInput {
  monthlyMinutesCapacity: number;
  expenses: OverheadExpenseItem[];
}

export interface OverheadRateResult {
  totalMonthlyOverheadEur: string;
  overheadRatePerMinuteEur: string;
}

export interface ProductUnitCostInput {
  name: string;
  packageQuantity: number;
  unit: string;
  packagePriceEur: string;
  wastagePct: string;
}

export interface ProductUnitCostResult {
  httpStatus: number;
  errorCode?: string;
  unitPriceEur?: string;
  effectiveUnitPriceWithWastageEur?: string;
}

export interface StaffUnitCostInput {
  roleName: string;
  monthlyMinutes: number;
  netSalaryEur: string;
  retentionPct: string;
  companyCoefficient: string;
}

export interface StaffUnitCostResult {
  httpStatus: number;
  errorCode?: string;
  companyMonthlyCostEur?: string;
  minuteRateEur?: string;
}

export interface EscandalloProductLine {
  name: string;
  doseQuantity: string;
  unitPriceEur: string;
  wastagePct: string;
}

export interface EscandalloServiceInput {
  serviceName: string;
  durationMinutes: number;
  overheadRatePerMinuteEur: string;
  staffMinuteRateEur: string;
  targetProfitPct: string;
  vatPct: string;
  productLines: EscandalloProductLine[];
}

export interface EscandalloServiceResult {
  httpStatus: number;
  errorCode?: string;
  allocatedOverheadEur?: string;
  staffCostEur?: string;
  productsCostEur?: string;
  totalEscandalloCostEur?: string;
  netProfitEur?: string;
  priceWithProfitExclVatEur?: string;
  calculatedPvpInclVatEur?: string;
}

export interface MarginSemaphoreInput {
  totalEscandalloCostEur: string;
  targetProfitPct: string;
  vatPct: string;
  fixedCommercialPvpEur: string;
}

export interface MarginSemaphoreResult {
  realPriceExclVatEur: string;
  realNetProfitEur: string;
  semaphoreStatus: 'OPTIMO' | 'ALERTA' | 'CRITICO';
}

export interface LinkedEscandalloItem {
  id: string;
  name: string;
  doseUsed: number;
  currentTotalEur: string;
  version: number;
}

export interface CostMutationCascadeInput {
  costId: string;
  costType: string;
  previousAmountEur: string;
  newAmountEur: string;
  packageQuantity: number;
  linkedEscandallos: LinkedEscandalloItem[];
}

export interface UpdatedEscandalloSnapshot {
  id: string;
  name: string;
  newTotalEur: string;
  version: number;
}

export interface CostMutationCascadeResult {
  costHistoryEntriesCreated: number;
  recalculatedEscandallosCount: number;
  updatedEscandallos: UpdatedEscandalloSnapshot[];
}

export interface ArchiveResourceInput {
  resourceId: string;
  hasHistoryOrUsage: boolean;
  requestedOperation: string;
}

export interface ArchiveResourceResult {
  httpStatus: number;
  errorCode?: string;
  newStatus: 'active' | 'archived';
}

const SCALE = 100_000_000n;
const HALF_SCALE = 50_000_000n;

function parseScaled(raw: string): bigint {
  const trimmed = raw.trim();
  const isNeg = trimmed.startsWith('-');
  const clean = isNeg ? trimmed.slice(1) : trimmed;
  const [intStr = '0', fracStr = ''] = clean.split('.');
  const paddedFrac = fracStr.slice(0, 8).padEnd(8, '0');
  const magnitude = BigInt(intStr) * SCALE + BigInt(paddedFrac);
  return isNeg ? -magnitude : magnitude;
}

function formatScaled(val: bigint, minDecimals = 2): string {
  const isNeg = val < 0n;
  const absVal = isNeg ? -val : val;
  const intPart = absVal / SCALE;
  const rawFrac = (absVal % SCALE).toString().padStart(8, '0');
  const trimmedFrac = rawFrac.replace(/0+$/, '');
  const finalFrac = trimmedFrac.length < minDecimals ? rawFrac.slice(0, minDecimals) : trimmedFrac;
  return `${isNeg ? '-' : ''}${intPart.toString()}.${finalFrac}`;
}

function mulScaled(a: bigint, b: bigint): bigint {
  return (a * b + HALF_SCALE) / SCALE;
}

function divScaled(a: bigint, b: bigint): bigint {
  return (a * SCALE + b / 2n) / b;
}

export function calculateOverheadRatePerMinute(input: OverheadRateInput): OverheadRateResult {
  const totalScaled = input.expenses.reduce((acc, item) => acc + parseScaled(item.monthlyCostEur), 0n);
  const capacityScaled = BigInt(input.monthlyMinutesCapacity) * SCALE;
  const rateScaled = divScaled(totalScaled, capacityScaled);
  return {
    totalMonthlyOverheadEur: formatScaled(totalScaled, 2),
    overheadRatePerMinuteEur: formatScaled(rateScaled, 2),
  };
}

function validateProductCostInput(input: ProductUnitCostInput): ProductUnitCostResult | null {
  if (!input.name.trim()) return { httpStatus: 400, errorCode: 'EMPTY_ITEM_NAME' };
  if (input.packageQuantity === 0) return { httpStatus: 422, errorCode: 'INVALID_ZERO_QUANTITY' };
  if (input.packageQuantity < 0) return { httpStatus: 422, errorCode: 'NEGATIVE_QUANTITY' };
  const price = Number(input.packagePriceEur);
  if (Number.isNaN(price) || price < 0) return { httpStatus: 422, errorCode: 'NEGATIVE_PRICE' };
  const wastage = Number(input.wastagePct);
  if (Number.isNaN(wastage) || wastage < 0 || wastage > 1) return { httpStatus: 422, errorCode: 'WASTAGE_OUT_OF_RANGE' };
  return null;
}

export function calculateProductUnitCost(input: ProductUnitCostInput): ProductUnitCostResult {
  const validationError = validateProductCostInput(input);
  if (validationError) return validationError;
  const priceScaled = parseScaled(input.packagePriceEur);
  const qtyScaled = BigInt(input.packageQuantity) * SCALE;
  const unitScaled = divScaled(priceScaled, qtyScaled);
  const wastageFactor = SCALE + parseScaled(input.wastagePct);
  const effectiveScaled = mulScaled(unitScaled, wastageFactor);
  return {
    httpStatus: 200,
    unitPriceEur: formatScaled(unitScaled, 2),
    effectiveUnitPriceWithWastageEur: formatScaled(effectiveScaled, 2),
  };
}

export function calculateStaffUnitCost(input: StaffUnitCostInput): StaffUnitCostResult {
  if (!input.roleName.trim()) return { httpStatus: 400, errorCode: 'EMPTY_ITEM_NAME' };
  if (input.monthlyMinutes <= 0) return { httpStatus: 422, errorCode: 'DIVISION_BY_ZERO_GUARD' };
  const netScaled = parseScaled(input.netSalaryEur);
  const retentionScaled = parseScaled(input.retentionPct);
  const coefScaled = parseScaled(input.companyCoefficient);
  const grossScaled = mulScaled(netScaled, retentionScaled) + netScaled;
  const companyMonthlyScaled = mulScaled(grossScaled, coefScaled);
  const minuteRateScaled = divScaled(companyMonthlyScaled, BigInt(input.monthlyMinutes) * SCALE);
  return {
    httpStatus: 200,
    companyMonthlyCostEur: formatScaled(companyMonthlyScaled, 2),
    minuteRateEur: formatScaled(minuteRateScaled, 2),
  };
}

function validateEscandalloInput(input: EscandalloServiceInput): EscandalloServiceResult | null {
  if (input.durationMinutes === 0) return { httpStatus: 422, errorCode: 'INVALID_SERVICE_DURATION' };
  if (input.durationMinutes < 0) return { httpStatus: 422, errorCode: 'NEGATIVE_SERVICE_DURATION' };
  if (input.productLines.length > 100) return { httpStatus: 422, errorCode: 'MAX_RECIPE_LINES_EXCEEDED' };
  const hasNegativeDose = input.productLines.some((line) => Number(line.doseQuantity) < 0);
  if (hasNegativeDose) return { httpStatus: 422, errorCode: 'NEGATIVE_PRODUCT_DOSE' };
  return null;
}

function sumRecipeLinesScaled(lines: EscandalloProductLine[]): bigint {
  return lines.reduce((acc, line) => {
    const doseUnit = mulScaled(parseScaled(line.doseQuantity), parseScaled(line.unitPriceEur));
    const lineTotal = mulScaled(doseUnit, SCALE + parseScaled(line.wastagePct));
    return acc + lineTotal;
  }, 0n);
}

export function calculateEscandalloService(input: EscandalloServiceInput): EscandalloServiceResult {
  const err = validateEscandalloInput(input);
  if (err) return err;
  const durationScaled = BigInt(input.durationMinutes) * SCALE;
  const overheadScaled = mulScaled(durationScaled, parseScaled(input.overheadRatePerMinuteEur));
  const staffScaled = mulScaled(durationScaled, parseScaled(input.staffMinuteRateEur));
  const productsScaled = sumRecipeLinesScaled(input.productLines);
  const totalCostScaled = overheadScaled + staffScaled + productsScaled;
  const netProfitScaled = mulScaled(totalCostScaled, parseScaled(input.targetProfitPct));
  const priceExclVatScaled = totalCostScaled + netProfitScaled;
  const pvpInclVatScaled = mulScaled(priceExclVatScaled, SCALE + parseScaled(input.vatPct));
  return {
    httpStatus: 200,
    allocatedOverheadEur: formatScaled(overheadScaled, 2),
    staffCostEur: formatScaled(staffScaled, 2),
    productsCostEur: formatScaled(productsScaled, 2),
    totalEscandalloCostEur: formatScaled(totalCostScaled, 2),
    netProfitEur: formatScaled(netProfitScaled, 2),
    priceWithProfitExclVatEur: formatScaled(priceExclVatScaled, 2),
    calculatedPvpInclVatEur: formatScaled(pvpInclVatScaled, 2),
  };
}

export function evaluateMarginSemaphore(input: MarginSemaphoreInput): MarginSemaphoreResult {
  const pvpScaled = parseScaled(input.fixedCommercialPvpEur);
  const vatDivisor = SCALE + parseScaled(input.vatPct);
  const realPriceExclVatScaled = divScaled(pvpScaled, vatDivisor);
  const totalCostScaled = parseScaled(input.totalEscandalloCostEur);
  const realNetProfitScaled = realPriceExclVatScaled - totalCostScaled;
  const targetProfitScaled = mulScaled(totalCostScaled, parseScaled(input.targetProfitPct));
  const semaphoreStatus =
    realNetProfitScaled <= 0n ? 'CRITICO' : realNetProfitScaled < targetProfitScaled ? 'ALERTA' : 'OPTIMO';
  return {
    realPriceExclVatEur: formatScaled(realPriceExclVatScaled, 2),
    realNetProfitEur: formatScaled(realNetProfitScaled, 2),
    semaphoreStatus,
  };
}

export async function applyCostMutationWithCascade(
  input: CostMutationCascadeInput,
): Promise<CostMutationCascadeResult> {
  if (input.linkedEscandallos.length === 0) {
    return { costHistoryEntriesCreated: 0, recalculatedEscandallosCount: 0, updatedEscandallos: [] };
  }
  const diffScaled = parseScaled(input.newAmountEur) - parseScaled(input.previousAmountEur);
  const deltaPerUnitScaled = divScaled(diffScaled, BigInt(input.packageQuantity) * SCALE);
  const updatedEscandallos = input.linkedEscandallos.map((esc) => {
    const deltaEsc = mulScaled(BigInt(esc.doseUsed) * SCALE, deltaPerUnitScaled);
    const nextTotal = parseScaled(esc.currentTotalEur) + deltaEsc;
    return {
      id: esc.id,
      name: esc.name,
      newTotalEur: formatScaled(nextTotal, 6),
      version: esc.version + 1,
    };
  });
  return {
    costHistoryEntriesCreated: 1,
    recalculatedEscandallosCount: updatedEscandallos.length,
    updatedEscandallos,
  };
}

export async function archiveCostOrService(input: ArchiveResourceInput): Promise<ArchiveResourceResult> {
  if (input.requestedOperation === 'sobrescribir_snapshot_historico_v1') {
    return { httpStatus: 403, errorCode: 'IMMUTABLE_HISTORY_VIOLATION', newStatus: 'active' };
  }
  if (input.hasHistoryOrUsage && input.requestedOperation.startsWith('borrado_fisico')) {
    return { httpStatus: 409, errorCode: 'PHYSICAL_DELETE_FORBIDDEN', newStatus: 'archived' };
  }
  return { httpStatus: 200, newStatus: 'archived' };
}
