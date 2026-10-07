-- =============================================================================
-- Migration: 001_initial_rls_schema.sql
-- Architecture Reference: ADR-003 (Multi-Tenant Isolation via PostgreSQL 16 RLS)
-- Security Requirements: SEC-REQ-AUTHZ-001, SEC-REQ-AUDIT-001, BR-RECALC-HISTORY-001
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 1. Centers (Tenants)
CREATE TABLE IF NOT EXISTS centers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_id UUID NOT NULL,
    name VARCHAR(160) NOT NULL,
    subscription_plan VARCHAR(20) NOT NULL DEFAULT 'FREE' CHECK (subscription_plan IN ('FREE', 'PRO')),
    stripe_customer_id VARCHAR(120),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Configurable Global Parameters & Cabin Dose Equivalences (OQ-01)
CREATE TABLE IF NOT EXISTS center_parameters (
    center_id UUID PRIMARY KEY REFERENCES centers(id) ON DELETE CASCADE,
    iva NUMERIC(6,4) NOT NULL DEFAULT 0.2100,
    beneficio_objetivo NUMERIC(6,4) NOT NULL DEFAULT 0.2500,
    horas_dia NUMERIC(6,2) NOT NULL DEFAULT 8.00,
    dias_mes NUMERIC(6,2) NOT NULL DEFAULT 22.00,
    minutos_mensuales NUMERIC(10,2) NOT NULL DEFAULT 10560.00,
    coeficiente_empresa NUMERIC(6,4) NOT NULL DEFAULT 1.3500,
    merma_pct NUMERIC(6,4) NOT NULL DEFAULT 0.0300,
    dose_equivalences_json JSONB NOT NULL DEFAULT '{"gota":0.05,"pulsacion":2.0,"cucharadita":5.0,"ampolla_vial":2.0}'::jsonb,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3. Unified Cost Catalog (Productos, Personal, Gastos Generales) with Logical Archiving (M1)
CREATE TABLE IF NOT EXISTS cost_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    center_id UUID NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    category VARCHAR(32) NOT NULL CHECK (category IN ('PRODUCTO', 'PERSONAL', 'GASTO_FIJO')),
    name VARCHAR(160) NOT NULL,
    container_ml NUMERIC(10,2),
    purchase_price NUMERIC(12,4) NOT NULL,
    default_dose_ml NUMERIC(10,4),
    archived BOOLEAN NOT NULL DEFAULT FALSE,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 4. Conditional Immutable Cost History (BR-RECALC-HISTORY-001)
CREATE TABLE IF NOT EXISTS cost_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    center_id UUID NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    cost_item_id UUID NOT NULL REFERENCES cost_items(id) ON DELETE RESTRICT,
    previous_value NUMERIC(12,4) NOT NULL,
    new_value NUMERIC(12,4) NOT NULL,
    changed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 5. Escandallos (Treatments/Services) with Commercial PVP & Logical Archiving (M1, OQ-02)
CREATE TABLE IF NOT EXISTS escandallos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    center_id UUID NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    name VARCHAR(160) NOT NULL,
    duration_minutes NUMERIC(8,2) NOT NULL CHECK (duration_minutes > 0),
    staff_role VARCHAR(80) NOT NULL,
    pvp_comercial_fijado NUMERIC(12,2) NOT NULL,
    archived BOOLEAN NOT NULL DEFAULT FALSE,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 6. Escandallo 1:N Recipe Lines (M3)
CREATE TABLE IF NOT EXISTS escandallo_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    center_id UUID NOT NULL REFERENCES centers(id) ON DELETE CASCADE,
    escandallo_id UUID NOT NULL REFERENCES escandallos(id) ON DELETE CASCADE,
    product_cost_id UUID NOT NULL REFERENCES cost_items(id) ON DELETE RESTRICT,
    dose_ml NUMERIC(10,4) NOT NULL CHECK (dose_ml > 0)
);

-- 7. Stripe Webhook Idempotency Ledger (SEC-REQ-BILLING-001)
CREATE TABLE IF NOT EXISTS stripe_webhook_events (
    event_id VARCHAR(128) PRIMARY KEY,
    event_type VARCHAR(80) NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- =============================================================================
-- Mandatory Row-Level Security (RLS) Enforcement (ADR-003 / SEC-REQ-AUTHZ-001)
-- =============================================================================

ALTER TABLE center_parameters ENABLE ROW LEVEL SECURITY;
ALTER TABLE center_parameters FORCE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_center_parameters ON center_parameters
    USING (center_id = current_setting('app.current_center_id', true)::uuid)
    WITH CHECK (center_id = current_setting('app.current_center_id', true)::uuid);

ALTER TABLE cost_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE cost_items FORCE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_cost_items ON cost_items
    USING (center_id = current_setting('app.current_center_id', true)::uuid)
    WITH CHECK (center_id = current_setting('app.current_center_id', true)::uuid);

ALTER TABLE cost_history ENABLE ROW LEVEL SECURITY;
ALTER TABLE cost_history FORCE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_cost_history ON cost_history
    USING (center_id = current_setting('app.current_center_id', true)::uuid)
    WITH CHECK (center_id = current_setting('app.current_center_id', true)::uuid);

ALTER TABLE escandallos ENABLE ROW LEVEL SECURITY;
ALTER TABLE escandallos FORCE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_escandallos ON escandallos
    USING (center_id = current_setting('app.current_center_id', true)::uuid)
    WITH CHECK (center_id = current_setting('app.current_center_id', true)::uuid);

ALTER TABLE escandallo_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE escandallo_items FORCE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_escandallo_items ON escandallo_items
    USING (center_id = current_setting('app.current_center_id', true)::uuid)
    WITH CHECK (center_id = current_setting('app.current_center_id', true)::uuid);
