---
id: TSK-PLAN-CHG-001-ESCANDALLO-MVP
type: task-plan
change-id: CHG-001-ESCANDALLO-MVP
title: 'Desglose de Tareas Verificables: MVP Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética'
version: 1.0.0
schema-version: '1.0'
status: draft
governance-summary:
  autonomous-tasks-count: 5
  human-review-plan-count: 3
  ambiguous-count: 0
  high-risk-manual-count: 0
tasks:
  - id: TSK-001
    title: 'Módulo IAM, RBAC, Guardia Multi-Tenant RLS y Rate Limiter (CMP-IAM-TENANT-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/iam-tenant-rls.spec.ts'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-002
    title: 'Motor de Catálogo de Costes, Plantilla Semilla y Fórmulas Unitarias con decimal.js (CMP-COST-ENGINE-001)'
    complexity: LOW
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/cost-engine.spec.ts'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-003
    title: 'Motor de Escandallo Total, Beneficio Neto, PVP Comercial y Semáforo de Margen M2/M5 (CMP-COST-ENGINE-001)'
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/escandallo-calc.spec.ts'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-004
    title: 'Recálculo Transaccional Síncrono en Cascada, Snapshots Inmutables y Baja Lógica M1 (CMP-COST-ENGINE-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/cascade-history.spec.ts'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-005
    title: 'Subsistema Documental Seguro: Importador Excel Anti-XXE/ZipBomb/DDE y Exportador PDF/XLSX (CMP-DOC-IO-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/doc-io-report.spec.ts'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-006
    title: 'Control Atómico de Cuota Freemium (SELECT FOR UPDATE) y Webhooks HMAC-SHA256 de Stripe (CMP-BILLING-STRIPE-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/billing-stripe.spec.ts'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-007
    title: 'Subsistema de Observabilidad Dual (Pino + OTel), Nivel en Caliente y Filtro [REDACTED] (CMP-OBS-TELEMETRY-001)'
    complexity: LOW
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/obs-telemetry.spec.ts'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-008
    title: 'Cliente Web Adaptativo (PC, Tablet, Móvil) y Accesible WCAG 2.1 AA con Constructor Dinámico M3 (CMP-WEB-UI-001)'
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'pnpm vitest run tests/unit/ui-a11y.spec.ts'
    assigned-to: agent-developer
    status: DONE
supersedes: null
superseded-by: null
---

# Desglose de Tareas Verificables: CHG-001-ESCANDALLO-MVP

## 1. Matriz de Clasificación de Autonomía y Supervisión Humana

| Modo de Autonomía | Semáforo | Criterio de Activación | Comportamiento del Agente y del Humano |
| :--- | :---: | :--- | :--- |
| **`AUTONOMOUS`** | 🟢 | Riesgo bajo, tarea aislada y bien especificada con pruebas inmediatas. | **Plan + Ejecución Autónoma**. El agente genera el plan y escribe el código sin interrupción. |
| **`HUMAN_REVIEW_PLAN`** | 🟡 | Riesgo medio, cambios en arquitectura, contratos de API o reglas críticas. | **Revisión Obligatoria de Plan**. El agente diseña el plan detallado y espera aprobación humana. |
| **`AMBIGUOUS`** | 🟠 | Requisitos vagos, criterios incompletos o conflicto de lógica de negocio. | **Bloqueada para Implementación**. Requiere clarificación previa con el usuario. |
| **`HIGH_RISK_MANUAL`** | 🔴 | Riesgo crítico (migraciones destructivas de DB, claves criptográficas, infra). | **Prohibida la Ejecución Autónoma**. Ejecución directa humana. |

---

## 2. Plan Detallado de Tareas y Criterios de Verificación

### TSK-001: Módulo IAM, RBAC, Guardia Multi-Tenant RLS y Rate Limiter (`CMP-IAM-TENANT-001`)
- **Requisitos cubiertos**: `FR-TENANT-001`, `SEC-REQ-TENANT-001`, `SEC-REQ-DOS-001`, `QR-SEC-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `pnpm vitest run tests/unit/iam-tenant-rls.spec.ts`

### TSK-002: Motor de Catálogo de Costes, Plantilla Semilla y Fórmulas Unitarias (`CMP-COST-ENGINE-001`)
- **Requisitos cubiertos**: `FR-COST-001`, `BR-CALC-FORMULAS-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `pnpm vitest run tests/unit/cost-engine.spec.ts`

### TSK-003: Motor de Escandallo Total, Beneficio Neto, PVP Comercial y Semáforo de Margen (`CMP-COST-ENGINE-001`)
- **Requisitos cubiertos**: `FR-CALC-001`, `BR-CALC-FORMULAS-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `pnpm vitest run tests/unit/escandallo-calc.spec.ts`

### TSK-004: Recálculo Transaccional Síncrono en Cascada, Snapshots Inmutables y Baja Lógica M1 (`CMP-COST-ENGINE-001`)
- **Requisitos cubiertos**: `FR-HIST-001`, `BR-RECALC-HISTORY-001`, `SEC-REQ-DOS-001`
- **Modo**: `HUMAN_REVIEW_PLAN` 🟡
- **Verificación**: `pnpm vitest run tests/unit/cascade-history.spec.ts`

### TSK-005: Subsistema Documental Seguro: Importador Excel Anti-XXE/ZipBomb/DDE y Exportador PDF/XLSX (`CMP-DOC-IO-001`)
- **Requisitos cubiertos**: `FR-REPORT-001`, `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`
- **Modo**: `HUMAN_REVIEW_PLAN` 🟡
- **Verificación**: `pnpm vitest run tests/unit/doc-io-report.spec.ts`

### TSK-006: Control Atómico de Cuota Freemium (`SELECT FOR UPDATE`) y Webhooks HMAC-SHA256 de Stripe (`CMP-BILLING-STRIPE-001`)
- **Requisitos cubiertos**: `FR-BILLING-001`, `SEC-REQ-BILLING-001`, `QR-SEC-001`, `BR-FREEMIUM-QUOTA-001`
- **Modo**: `HUMAN_REVIEW_PLAN` 🟡
- **Verificación**: `pnpm vitest run tests/unit/billing-stripe.spec.ts`

### TSK-007: Subsistema de Observabilidad Dual (Pino + OTel), Nivel en Caliente y Filtro `[REDACTED]` (`CMP-OBS-TELEMETRY-001`)
- **Requisitos cubiertos**: `FR-OBS-001`, `SEC-REQ-OBS-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `pnpm vitest run tests/unit/obs-telemetry.spec.ts`

### TSK-008: Cliente Web Adaptativo (PC, Tablet, Móvil) y Accesible WCAG 2.1 AA con Constructor Dinámico M3 (`CMP-WEB-UI-001`)
- **Requisitos cubiertos**: `QR-UI-A11Y-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `pnpm vitest run tests/unit/ui-a11y.spec.ts`

---

## 3. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-qa-engineer | Desglose de 8 tareas atómicas vinculadas a suites de prueba en rojo | CHG-001-ESCANDALLO-MVP |

