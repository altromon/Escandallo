export interface HotLogLevelInput {
  actorRole: string;
  newLevel: string;
}

export interface HotLogLevelResult {
  httpStatus: number;
  appliedWithoutRestart: boolean;
  errorCode?: string;
}

export interface TelemetryBundleInput {
  actorRole: string;
  resource: string;
  windowHours: number;
}

export interface TelemetryBundleResult {
  httpStatus: number;
  outputFormat?: string;
  errorCode?: string;
}

export interface RawTelemetryRecordInput {
  logLevel: string;
  authorization: string;
  cookie: string;
  stripe_signature: string;
  searchQuery: string;
}

export interface RedactedTelemetryRecordResult {
  authorization: string;
  cookie: string;
  stripe_signature: string;
  singleLineJson: string;
}

const VALID_LOG_LEVELS = new Set(['DEBUG', 'INFO', 'WARN', 'ERROR']);
const FORMAT_BY_RESOURCE: Record<string, string> = {
  logs: 'ndjson',
  traces: 'otlp_json',
  metrics: 'prometheus_csv',
};

export async function setHotLogLevel(input: HotLogLevelInput): Promise<HotLogLevelResult> {
  if (input.actorRole !== 'ACT-MAINTAINER-001') {
    return { httpStatus: 403, appliedWithoutRestart: false, errorCode: 'MAINTAINER_ROLE_REQUIRED' };
  }
  if (!VALID_LOG_LEVELS.has(input.newLevel)) {
    return { httpStatus: 400, appliedWithoutRestart: false, errorCode: 'INVALID_LOG_LEVEL' };
  }
  return { httpStatus: 200, appliedWithoutRestart: true };
}

export async function exportTelemetryBundle(input: TelemetryBundleInput): Promise<TelemetryBundleResult> {
  if (input.actorRole !== 'ACT-MAINTAINER-001') {
    return { httpStatus: 403, errorCode: 'MAINTAINER_ROLE_REQUIRED' };
  }
  if (input.windowHours < 1 || input.windowHours > 720) {
    return { httpStatus: 422, errorCode: 'WINDOW_HOURS_OUT_OF_RANGE' };
  }
  const outputFormat = FORMAT_BY_RESOURCE[input.resource] ?? 'ndjson';
  return { httpStatus: 200, outputFormat };
}

export function redactTelemetryRecord(input: RawTelemetryRecordInput): RedactedTelemetryRecordResult {
  const sanitizedQuery = input.searchQuery.replace(/[\r\n]+/g, ' ');
  const payload = {
    level: input.logLevel,
    authorization: '[REDACTED]',
    cookie: '[REDACTED]',
    stripe_signature: '[REDACTED]',
    searchQuery: sanitizedQuery,
  };
  return {
    authorization: '[REDACTED]',
    cookie: '[REDACTED]',
    stripe_signature: '[REDACTED]',
    singleLineJson: JSON.stringify(payload),
  };
}
