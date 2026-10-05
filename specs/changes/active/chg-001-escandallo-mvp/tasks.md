---
id: TSK-PLAN-CHG-001-ESCANDALLO-MVP
type: task-plan
change-id: CHG-001-ESCANDALLO-MVP
title: 'Desglose de Tareas Verificables en C# (.NET): MVP Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética'
version: 1.1.0
schema-version: '1.0'
status: draft
governance-summary:
  autonomous-tasks-count: 5
  human-review-plan-count: 3
  ambiguous-count: 0
  high-risk-manual-count: 0
tasks:
  - id: TSK-001
    title: 'Módulo IAM, RBAC, Guardia Multi-Tenant RLS y Rate Limiter en C# (CMP-IAM-TENANT-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~IamTenantRlsTests'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-002
    title: 'Motor de Catálogo de Costes, Plantilla Semilla y Fórmulas Unitarias con System.Decimal en C# (CMP-COST-ENGINE-001)'
    complexity: LOW
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~CostEngineTests'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-003
    title: 'Motor de Escandallo Total, Beneficio Neto, PVP Comercial y Semáforo de Margen M2/M5 en C# (CMP-COST-ENGINE-001)'
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~EscandalloCalcTests'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-004
    title: 'Recálculo Transaccional Síncrono en Cascada, Snapshots Inmutables y Baja Lógica M1 en C# (CMP-COST-ENGINE-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~CascadeHistoryTests'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-005
    title: 'Subsistema Documental Seguro: Importador Excel Anti-XXE/ZipBomb/DDE y Exportador PDF/XLSX en C# (CMP-DOC-IO-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~DocIoReportTests'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-006
    title: 'Control Atómico de Cuota Freemium (SELECT FOR UPDATE) y Webhooks HMAC-SHA256 de Stripe en C# (CMP-BILLING-STRIPE-001)'
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~BillingStripeTests'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-007
    title: 'Subsistema de Observabilidad Dual, Nivel en Caliente y Filtro [REDACTED] en C# (CMP-OBS-TELEMETRY-001)'
    complexity: LOW
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~ObsTelemetryTests'
    assigned-to: agent-developer
    status: DONE
  - id: TSK-008
    title: 'Cliente Web Adaptativo (PC, Tablet, Móvil) y Accesible WCAG 2.1 AA con Constructor Dinámico M3 en C# (CMP-WEB-UI-001)'
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: 'dotnet test --filter FullyQualifiedName~UiA11yTests'
    assigned-to: agent-developer
    status: DONE
supersedes: null
superseded-by: null
---

# Desglose de Tareas Verificables en C# (.NET): CHG-001-ESCANDALLO-MVP

## 1. Matriz de Clasificación de Autonomía y Supervisión Humana

| Modo de Autonomía | Semáforo | Criterio de Activación | Comportamiento del Agente y del Humano |
| :--- | :---: | :--- | :--- |
| **`AUTONOMOUS`** | 🟢 | Riesgo bajo, tarea aislada y bien especificada con pruebas inmediatas. | **Plan + Ejecución Autónoma**. El agente genera el plan y escribe el código sin interrupción. |
| **`HUMAN_REVIEW_PLAN`** | 🟡 | Riesgo medio, cambios en arquitectura, contratos de API o reglas críticas. | **Revisión Obligatoria de Plan**. El agente diseña el plan detallado y espera aprobación humana. |
| **`AMBIGUOUS`** | 🟠 | Requisitos vagos, criterios incompletos o conflicto de lógica de negocio. | **Bloqueada para Implementación**. Requiere clarificación previa con el usuario. |
| **`HIGH_RISK_MANUAL`** | 🔴 | Riesgo crítico (migraciones destructivas de DB, claves criptográficas, infra). | **Prohibida la Ejecución Autónoma**. Ejecución directa humana. |

---

## 2. Plan Detallado de Tareas y Criterios de Verificación (`dotnet test`)

### TSK-001: Módulo IAM, RBAC, Guardia Multi-Tenant RLS y Rate Limiter en C# (`CMP-IAM-TENANT-001`)
- **Requisitos cubiertos**: `FR-TENANT-001`, `SEC-REQ-TENANT-001`, `SEC-REQ-DOS-001`, `QR-SEC-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `dotnet test --filter FullyQualifiedName~IamTenantRlsTests`

### TSK-002: Motor de Catálogo de Costes, Plantilla Semilla y Fórmulas Unitarias con `System.Decimal` (`CMP-COST-ENGINE-001`)
- **Requisitos cubiertos**: `FR-COST-001`, `BR-CALC-FORMULAS-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `dotnet test --filter FullyQualifiedName~CostEngineTests`

### TSK-003: Motor de Escandallo Total, Beneficio Neto, PVP Comercial y Semáforo de Margen en C# (`CMP-COST-ENGINE-001`)
- **Requisitos cubiertos**: `FR-CALC-001`, `BR-CALC-FORMULAS-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `dotnet test --filter FullyQualifiedName~EscandalloCalcTests`

### TSK-004: Recálculo Transaccional Síncrono en Cascada, Snapshots Inmutables y Baja Lógica M1 en C# (`CMP-COST-ENGINE-001`)
- **Requisitos cubiertos**: `FR-HIST-001`, `BR-RECALC-HISTORY-001`, `SEC-REQ-DOS-001`
- **Modo**: `HUMAN_REVIEW_PLAN` 🟡
- **Verificación**: `dotnet test --filter FullyQualifiedName~CascadeHistoryTests`

### TSK-005: Subsistema Documental Seguro: Importador Excel Anti-XXE/ZipBomb/DDE y Exportador PDF/XLSX en C# (`CMP-DOC-IO-001`)
- **Requisitos cubiertos**: `FR-REPORT-001`, `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`
- **Modo**: `HUMAN_REVIEW_PLAN` 🟡
- **Verificación**: `dotnet test --filter FullyQualifiedName~DocIoReportTests`

### TSK-006: Control Atómico de Cuota Freemium (`SELECT FOR UPDATE`) y Webhooks HMAC-SHA256 de Stripe en C# (`CMP-BILLING-STRIPE-001`)
- **Requisitos cubiertos**: `FR-BILLING-001`, `SEC-REQ-BILLING-001`, `QR-SEC-001`, `BR-FREEMIUM-QUOTA-001`
- **Modo**: `HUMAN_REVIEW_PLAN` 🟡
- **Verificación**: `dotnet test --filter FullyQualifiedName~BillingStripeTests`

### TSK-007: Subsistema de Observabilidad Dual, Nivel en Caliente y Filtro `[REDACTED]` en C# (`CMP-OBS-TELEMETRY-001`)
- **Requisitos cubiertos**: `FR-OBS-001`, `SEC-REQ-OBS-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `dotnet test --filter FullyQualifiedName~ObsTelemetryTests`

### TSK-008: Cliente Web Adaptativo (PC, Tablet, Móvil) y Accesible WCAG 2.1 AA con Constructor Dinámico M3 en C# (`CMP-WEB-UI-001`)
- **Requisitos cubiertos**: `QR-UI-A11Y-001`
- **Modo**: `AUTONOMOUS` 🟢
- **Verificación**: `dotnet test --filter FullyQualifiedName~UiA11yTests`

---

## 3. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-qa-engineer | Desglose de 8 tareas atómicas vinculadas a suites de prueba en rojo | CHG-001-ESCANDALLO-MVP |
| **1.1.0** | 2026-10-05 | agent-developer | Migración a C# (.NET + System.Decimal + xUnit) y completado de TSK-001 a TSK-008 | CHG-001-ESCANDALLO-MVP |
