export interface CrossCenterCopyInput {
  authenticatedUserEmail: string;
  sourceCenterId: string;
  targetCenterId: string;
  ownerByCenter: Record<string, string>;
  catalogItemsCount: number;
}

export interface CrossCenterCopyResult {
  httpStatus: number;
  copiedItems: number;
  isolatedForeignCenters: string[];
  auditEvent?: {
    severity: 'SECURITY' | 'WARN' | 'INFO';
    reason: string;
  };
}

export interface SessionRlsInput {
  authenticatedUserId: string;
  activeCenterId: string;
  targetCenterId: string;
  operation: string;
}

export interface SessionRlsResult {
  httpStatus: number;
  leakedTenantData: undefined;
  auditSeverity?: 'SECURITY' | 'WARN' | 'INFO';
}

export interface TenantRateLimitInput {
  tenantId: string;
  operation: string;
  requestsInWindow: number;
  maxAllowedPerMinute: number;
}

export interface TenantRateLimitResult {
  httpStatus: number;
  retryAfterSeconds: number;
  logSeverity: 'INFO' | 'WARN' | 'SECURITY';
}

const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const MAX_CATALOG_COPY_ITEMS = 5000;

function listForeignCenters(ownerByCenter: Record<string, string>, email: string): string[] {
  return Object.entries(ownerByCenter)
    .filter(([, owner]) => owner !== email)
    .map(([centerId]) => centerId);
}

function isCenterOwnedBy(ownerByCenter: Record<string, string>, centerId: string, email: string): boolean {
  return ownerByCenter[centerId] === email;
}

export async function authorizeCrossCenterCopy(input: CrossCenterCopyInput): Promise<CrossCenterCopyResult> {
  const isolatedForeignCenters = listForeignCenters(input.ownerByCenter, input.authenticatedUserEmail);
  if (!input.sourceCenterId.trim() || !input.targetCenterId.trim()) {
    return { httpStatus: 400, copiedItems: 0, isolatedForeignCenters };
  }
  const ownsBoth =
    isCenterOwnedBy(input.ownerByCenter, input.sourceCenterId, input.authenticatedUserEmail) &&
    isCenterOwnedBy(input.ownerByCenter, input.targetCenterId, input.authenticatedUserEmail);
  if (!ownsBoth) {
    return {
      httpStatus: 403,
      copiedItems: 0,
      isolatedForeignCenters,
      auditEvent: { severity: 'SECURITY', reason: 'CROSS_TENANT_IDOR_ATTEMPT' },
    };
  }
  if (input.catalogItemsCount < 0 || input.catalogItemsCount > MAX_CATALOG_COPY_ITEMS) {
    return { httpStatus: 422, copiedItems: 0, isolatedForeignCenters };
  }
  return { httpStatus: 200, copiedItems: input.catalogItemsCount, isolatedForeignCenters };
}

export async function verifySessionAndRls(input: SessionRlsInput): Promise<SessionRlsResult> {
  if (!UUID_REGEX.test(input.targetCenterId) || !UUID_REGEX.test(input.activeCenterId)) {
    return { httpStatus: 400, leakedTenantData: undefined };
  }
  if (input.activeCenterId !== input.targetCenterId) {
    const httpStatus = input.operation.startsWith('PATCH') ? 404 : 403;
    return { httpStatus, leakedTenantData: undefined, auditSeverity: 'SECURITY' };
  }
  return { httpStatus: 200, leakedTenantData: undefined };
}

export function checkTenantRateLimit(input: TenantRateLimitInput): TenantRateLimitResult {
  if (input.requestsInWindow > input.maxAllowedPerMinute) {
    const logSeverity = input.operation === 'actualizacion_coste_con_cascada' ? 'WARN' : 'SECURITY';
    return { httpStatus: 429, retryAfterSeconds: 60, logSeverity };
  }
  return { httpStatus: 200, retryAfterSeconds: 0, logSeverity: 'INFO' };
}
