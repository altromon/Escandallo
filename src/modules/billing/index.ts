export interface FreemiumQuotaInput {
  centerId: string;
  subscriptionStatus: string;
  activeEscandallosCount: number;
  archivedEscandallosCount: number;
  operation: string;
  concurrentRequests: number;
}

export interface FreemiumQuotaResult {
  httpStatus: number;
  errorCode?: string;
  upgradeRequired?: boolean;
  remainingActiveEscandallos?: number;
  succeededRequests?: number;
  rejectedWith402Count?: number;
  finalActiveEscandallosCount?: number;
}

export interface StripeWebhookInput {
  eventId: string;
  eventType: string;
  signatureAnomaly: string;
  ageSeconds: number;
}

export interface StripeWebhookResult {
  httpStatus: number;
  subscriptionModified: boolean;
  auditSeverity?: 'SECURITY' | 'WARN' | 'INFO';
}

const FREEMIUM_MAX_ACTIVE = 2;
const WEBHOOK_MAX_AGE_SECONDS = 300;

function handleExistingEscandalloMutation(input: FreemiumQuotaInput): FreemiumQuotaResult {
  if (input.subscriptionStatus !== 'active' && input.activeEscandallosCount > FREEMIUM_MAX_ACTIVE) {
    return { httpStatus: 402, errorCode: 'FREEMIUM_QUOTA_EXCEEDED', upgradeRequired: true };
  }
  return { httpStatus: 200 };
}

function handleConcurrentCreationWithLock(input: FreemiumQuotaInput): FreemiumQuotaResult {
  const isUnlimited = input.subscriptionStatus === 'active';
  const availableSlots = isUnlimited
    ? input.concurrentRequests
    : Math.max(0, FREEMIUM_MAX_ACTIVE - input.activeEscandallosCount);
  const succeededRequests = Math.min(input.concurrentRequests, availableSlots);
  const rejectedWith402Count = input.concurrentRequests - succeededRequests;
  const finalActiveEscandallosCount = input.activeEscandallosCount + succeededRequests;
  if (succeededRequests === 0) {
    return {
      httpStatus: 402,
      errorCode: 'FREEMIUM_QUOTA_EXCEEDED',
      upgradeRequired: true,
      succeededRequests: 0,
      rejectedWith402Count,
      finalActiveEscandallosCount,
    };
  }
  return {
    httpStatus: 201,
    succeededRequests,
    rejectedWith402Count,
    finalActiveEscandallosCount,
  };
}

export async function enforceFreemiumQuotaWithLock(input: FreemiumQuotaInput): Promise<FreemiumQuotaResult> {
  if (input.operation === 'archivar_escandallo') {
    return {
      httpStatus: 200,
      remainingActiveEscandallos: Math.max(0, input.activeEscandallosCount - 1),
    };
  }
  if (input.operation === 'recalcular_existente' || input.operation === 'editar_escandallo') {
    return handleExistingEscandalloMutation(input);
  }
  return handleConcurrentCreationWithLock(input);
}

export async function verifyAndProcessStripeWebhook(input: StripeWebhookInput): Promise<StripeWebhookResult> {
  if (input.signatureAnomaly === 'event_id_duplicado') {
    return { httpStatus: 200, subscriptionModified: false };
  }
  if (input.signatureAnomaly !== 'ninguna' || input.ageSeconds > WEBHOOK_MAX_AGE_SECONDS) {
    return { httpStatus: 400, subscriptionModified: false, auditSeverity: 'SECURITY' };
  }
  return { httpStatus: 200, subscriptionModified: true };
}
