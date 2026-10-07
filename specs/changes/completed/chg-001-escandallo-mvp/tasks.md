---
id: TSK-PLAN-CHG-001-ESCANDALLO-MVP
type: task-plan
change-id: CHG-001-ESCANDALLO-MVP
title: 'Desglose de Tareas Verificables: MVP Plataforma SaaS Multi-Tenant de Escandallos en .NET Puro'
version: 1.0.0
schema-version: '1.0'
status: draft
governance-summary:
  autonomous-tasks-count: 5
  human-review-plan-count: 2
  ambiguous-count: 0
  high-risk-manual-count: 0
tasks:
  - id: TSK-001
    title: Contratos de Dominio en C# y Motor Decimal de Costes y Escandallos (CMP-COST-ENGINE-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~CostCatalogTests|FullyQualifiedName~EscandalloCalculatorTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-002
    title: Previsualización de Impacto M3, Recálculo en Cascada e Histórico Condicional (BR-RECALC-HISTORY-001)
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~CascadeHistoryTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-003
    title: Guardia Multi-Tenant RLS, Copia Dual M4 y Throttling Anti-DoS (CMP-IAM-TENANT-001)
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~TenantIsolationSecurityTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-004
    title: Importador Seguro .xlsx Anti-XXE/ZipBomb/DDE e Informes Ejecutivos PDFsharp/ClosedXML (CMP-DOC-IO-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~ExecutiveReportTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-005
    title: Cuota Freemium Atómica FOR UPDATE, Archivado M1 y Webhooks HMAC de Stripe.net (CMP-BILLING-STRIPE-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~StripeBillingQuotaTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-006
    title: Telemetría Serilog/OpenTelemetry con Nivel en Caliente, Redactado DEBUG y Validador WCAG/Perimetral (CMP-OBS-TELEMETRY-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~ObservabilityAndUiTests|FullyQualifiedName~SecurityHardeningCryptoTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-007
    title: Interfaz Web Adaptativa Blazor WCAG 2.1 AA con Catálogo 7 Categorías, Constructor Dinámico 1:N, Semáforo PVP y Modales M1/M3/M4/M5 (CMP-WEB-UI-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~WebUiComponentTests"
    assigned-to: agent-developer
    status: COMPLETED
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

## 2. Plan Detallado de Tareas y Criterios de Verificación (.NET 8/9 LTS)

### Fase 1: Dominio Financiero y Recálculo en Cascada (`TSK-001`, `TSK-002`)
- **`TSK-001` (`AUTONOMOUS` 🟢)**: Implementar en C# puro (`System.Decimal`) el motor de cálculo de costes unitarios (`FR-COST-001`, M5) y de escandallos, beneficio neto, simulación de PVP comercial y Semáforo de Erosión de Margen (`FR-CALC-001`, M2, M3). Verificación: `dotnet test --filter "FullyQualifiedName~CostCatalogTests|FullyQualifiedName~EscandalloCalculatorTests"`.
- **`TSK-002` (`HUMAN_REVIEW_PLAN` 🟡)**: Implementar la previsualización de impacto M3, el recálculo síncrono en cascada con histórico condicional inmutable (`FR-HIST-001`, `BR-RECALC-HISTORY-001`) y la protección de baja lógica (`archived`, M1). Verificación: `dotnet test --filter "FullyQualifiedName~CascadeHistoryTests"`.

### Fase 2: Seguridad Multi-Tenant, Documentos y Facturación Stripe (`TSK-003`, `TSK-004`, `TSK-005`)
- **`TSK-003` (`HUMAN_REVIEW_PLAN` 🟡)**: Implementar `CMP-IAM-TENANT-001` con aislamiento `PostgreSQL FORCE RLS`, validación dual de titularidad en copia de catálogo M4 (`FR-TENANT-001`, `SEC-REQ-TENANT-001`) y limitadores de tasa/concurrencia (`SEC-REQ-DOS-001`). Verificación: `dotnet test --filter "FullyQualifiedName~TenantIsolationSecurityTests"`.
- **`TSK-004` (`AUTONOMOUS` 🟢)**: Implementar `CMP-DOC-IO-001` con inspector ZIP/XML anti-XXE/ZipBomb, sanitizador de fórmulas DDE (`SEC-REQ-XLSX-001`) y exportadores exclusivos `.pdf` (`PDFsharp`) y `.xlsx` (`ClosedXML`) (`FR-REPORT-001`). Verificación: `dotnet test --filter "FullyQualifiedName~ExecutiveReportTests"`.
- **`TSK-005` (`AUTONOMOUS` 🟢)**: Implementar `CMP-BILLING-STRIPE-001` con bloqueo pesimista `FOR UPDATE` para la cuota de 2 escandallos activos, bloqueo/desbloqueo por archivado M1 y verificación HMAC-SHA256 ($\le 300\text{ s}$) e idempotencia con `Stripe.net` (`FR-BILLING-001`, `SEC-REQ-BILLING-001`). Verificación: `dotnet test --filter "FullyQualifiedName~StripeBillingQuotaTests"`.

### Fase 3: Observabilidad Dual y Cliente Web Blazor WCAG 2.1 AA (`TSK-006`, `TSK-007`)
- **`TSK-006` (`AUTONOMOUS` 🟢)**: Implementar `CMP-OBS-TELEMETRY-001` (`Serilog` + `OpenTelemetry .NET`, `LoggingLevelSwitch` en caliente con TTL, redactado inmutable `[REDACTED]` en `DEBUG` y validador de umbrales de seguridad/WCAG, `FR-OBS-001`, `SEC-REQ-OBS-001`, `QR-SEC-001`). Verificación: `dotnet test --filter "FullyQualifiedName~ObservabilityAndUiTests"`.
- **`TSK-007` (`AUTONOMOUS` 🟢)**: Implementar los componentes Razor de `CMP-WEB-UI-001` en Blazor Web App (`MainResponsiveLayout.razor`, `CostCatalogPage.razor`, `EscandalloBuilderPage.razor`, `CascadeImpactModal.razor`, `FreemiumPaywallModal.razor`) cumpliendo WCAG 2.1 AA, adaptación responsiva sin scroll horizontal (`portrait`/`landscape`) y regiones `aria-live="polite"` (`QR-UI-A11Y-001`, M1–M5). Verificación: `dotnet test --filter "FullyQualifiedName~WebUiComponentTests"`.

---

## 3. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-06 | agent-qa-engineer | Plan de tareas atómicas en .NET Puro con gobernanza y suites xUnit/BDD | CHG-001-ESCANDALLO-MVP |
