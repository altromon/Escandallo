---
id: TSK-PLAN-CHG-001-ESCANDALLO-MVP
type: task-plan
change-id: CHG-001-ESCANDALLO-MVP
title: 'Desglose de Tareas Verificables: MVP Plataforma SaaS Multi-Tenant de Escandallos en .NET 10 LTS + Blazor'
version: 1.7.0
schema-version: '1.0'
status: draft
governance-summary:
  autonomous-tasks-count: 5
  human-review-plan-count: 2
  ambiguous-count: 0
  high-risk-manual-count: 0
tasks:
  - id: TSK-001
    title: Contratos de Dominio en C# y Motor Decimal de Costes y Escandallos con Guardas Anti-Overflow (CMP-COST-ENGINE-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~CostCatalogTests|FullyQualifiedName~EscandalloCalculatorTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-002
    title: Previsualización de Impacto M3, Recálculo en Cascada e Histórico Condicional Inmutable (BR-RECALC-HISTORY-001)
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~CascadeHistoryTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-003
    title: Guardia Multi-Tenant SET LOCAL + FORCE RLS, Revalidación Blazor WSS, Copia Dual M4 y Throttling Anti-DoS (CMP-IAM-TENANT-001)
    complexity: MEDIUM
    risk-level: MEDIUM
    autonomy-mode: HUMAN_REVIEW_PLAN
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~TenantIsolationSecurityTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-004
    title: Importador Seguro .xlsx con BoundedReadStream Anti-XXE/ZipBomb, Escape DDE XLDataType.Text e Informes Ejecutivos PDFsharp/ClosedXML con Resumen Ejecutivo y Fichas Desglosadas para Dirección (CMP-DOC-IO-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~ExecutiveReportTests|FullyQualifiedName~WebPersistenceAndReportBinaryTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-005
    title: Cuota Freemium Atómica FOR UPDATE, Archivado M1 y Webhooks HMAC sobre Stream Crudo de Stripe.net (CMP-BILLING-STRIPE-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~StripeBillingQuotaTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-006
    title: Telemetría Serilog/OpenTelemetry con Nivel en Caliente, ToMaximumDepth(3), Redactado Exception.Message y Canal Inmutable SECURITY (CMP-OBS-TELEMETRY-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~ObservabilityAndUiTests|FullyQualifiedName~SecurityHardeningCryptoTests"
    assigned-to: agent-developer
    status: COMPLETED
  - id: TSK-007
    title: Interfaz Web Adaptativa Blazor WCAG 2.1 AA con Punto 1 al 100% del Ancho de Pantalla, Servicios Activos Colapsados en Una Línea e Histórico de Archivados por Fecha Descendente (CMP-WEB-UI-001)
    complexity: MEDIUM
    risk-level: LOW
    autonomy-mode: AUTONOMOUS
    verification:
      method: automated-unit-test
      command-or-criteria: dotnet test --filter "FullyQualifiedName~WebUiComponentTests|FullyQualifiedName~WebAppEndToEndUiTests|FullyQualifiedName~WebUiInteractiveFlowsE2ETests"
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

## 2. Plan Detallado de Tareas y Criterios de Verificación (.NET 10 LTS + Blazor)

### Fase 1: Dominio Financiero y Recálculo en Cascada (`TSK-001`, `TSK-002`)
- **`TSK-001` (`AUTONOMOUS` 🟢)**: Implementar en C# puro (`System.Decimal` 128-bit con cotas anti-`OverflowException`) el motor de cálculo de costes unitarios (`FR-COST-001`, `SEC-REQ-XLSX-001`, M5) y de escandallos, beneficio neto, simulación de PVP comercial y Semáforo de Erosión de Margen (`FR-CALC-001`, M2, M3). Verificación: `dotnet test --filter "FullyQualifiedName~CostCatalogTests|FullyQualifiedName~EscandalloCalculatorTests"`.
- **`TSK-002` (`HUMAN_REVIEW_PLAN` 🟡)**: Implementar la previsualización de impacto M3, el recálculo síncrono en cascada con histórico condicional inmutable (`FR-HIST-001`, `BR-RECALC-HISTORY-001`) y la protección de baja lógica (`archived`, M1). Verificación: `dotnet test --filter "FullyQualifiedName~CascadeHistoryTests"`.

### Fase 2: Seguridad Multi-Tenant, Documentos y Facturación Stripe (`TSK-003`, `TSK-004`, `TSK-005`)
- **`TSK-003` (`HUMAN_REVIEW_PLAN` 🟡)**: Implementar `CMP-IAM-TENANT-001` con aislamiento `PostgreSQL FORCE RLS` (`SET LOCAL` en pool `Npgsql` + rol `NOBYPASSRLS`), revalidación de parámetros en circuitos Blazor WSS, validación dual de titularidad en copia de catálogo M4 (`FR-TENANT-001`, `SEC-REQ-TENANT-001`) y limitadores de tasa/concurrencia/circuitos WSS (`SEC-REQ-DOS-001`). Verificación: `dotnet test --filter "FullyQualifiedName~TenantIsolationSecurityTests"`.
- **`TSK-004` (`AUTONOMOUS` 🟢)**: Implementar `CMP-DOC-IO-001` con inspector ZIP/XML anti-XXE/ZipBomb basado en `BoundedReadStream` ($\le 10\text{ MB}$ reales sobre `RecyclableMemoryStream`), sanitizador de fórmulas DDE con `TrimStart()` + `'` + `XLDataType.Text` (`SEC-REQ-XLSX-001`) y exportadores exclusivos `.pdf` (`PDFsharp`) y `.xlsx` (`ClosedXML`) acotados por `SemaphoreSlim(2, 2)` (`FR-REPORT-001`, `SEC-REQ-DOS-001`). **El generador PDF (`RealPdfReportGenerator` + `EscandalloExportRoutes`) debe producir un informe con formato ejecutivo para presentar a Dirección** (`CMP-DOC-IO-001` v1.2.0):
  1. **Resumen Ejecutivo inicial**: Cabecera del centro, parámetros financieros vigentes y tabla consolidada de todos los servicios activos con la información que se muestra en la vista colapsada (**Servicio**, **Duración en min**, **Personal asignado**, **Coste total €**, **Precio de venta fijado €**, **Precio recomendado €** y **Estado de rentabilidad** en lenguaje ejecutivo).
  2. **Listado detallado de cada servicio desglosado para Dirección**: Ficha individual por cada servicio activo con su desglose de costes (**Productos con merma**, **Personal imputado**, **Gastos Generales Fijos**, **Escandallo Total**), comparativa de rentabilidad (**Beneficio Neto Objetivo**, **Precio sin IVA**, **PVP Recomendado** vs. **PVP Comercial Fijado**, **Beneficio Neto Real** y **% Margen Real**) y tabla desglosada de los **productos cosméticos de su receta** (producto, dosis en ml, precio/formato de envase y coste unitario con merma).
  Verificación: `dotnet test --filter "FullyQualifiedName~ExecutiveReportTests|FullyQualifiedName~WebPersistenceAndReportBinaryTests"`.
- **`TSK-005` (`AUTONOMOUS` 🟢)**: Implementar `CMP-BILLING-STRIPE-001` con bloqueo pesimista `FOR UPDATE` para la cuota de 2 escandallos activos, vinculación en servidor de `metadata.center_id`, bloqueo/desbloqueo por archivado M1 y verificación `Stripe.EventUtility.ConstructEvent` sobre stream crudo ($\le 64\text{ KB}$, HMAC-SHA256 $\le 300\text{ s}$) e idempotencia monótona (`FR-BILLING-001`, `SEC-REQ-BILLING-001`). Verificación: `dotnet test --filter "FullyQualifiedName~StripeBillingQuotaTests"`.

### Fase 3: Observabilidad Dual y Cliente Web Blazor WCAG 2.1 AA (`TSK-006`, `TSK-007`)
- **`TSK-006` (`AUTONOMOUS` 🟢)**: Implementar `CMP-OBS-TELEMETRY-001` (`Serilog` + `OpenTelemetry .NET`, `LoggingLevelSwitch` en caliente con TTL de $60\text{ min}$, `Destructuring.ToMaximumDepth(3)`, redactado inmutable `[REDACTED]` incluido `Exception.Message`, canal protegido para eventos `SECURITY` y validador de umbrales de seguridad/WCAG, `FR-OBS-001`, `SEC-REQ-OBS-001`, `QR-SEC-001`). Verificación: `dotnet test --filter "FullyQualifiedName~ObservabilityAndUiTests|FullyQualifiedName~SecurityHardeningCryptoTests"`.
- **`TSK-007` (`AUTONOMOUS` 🟢)**: Implementar los componentes Razor de `CMP-WEB-UI-001` en Blazor Web App (`MainResponsiveLayout.razor`, `CostCatalogPage.razor`, `EscandalloBuilderPage.razor`, `CascadeImpactModal.razor`, `FreemiumPaywallModal.razor`), `wwwroot/index.html` y `wwwroot/app.css` cumpliendo WCAG 2.1 AA, adaptación responsiva sin scroll horizontal (`portrait`/`landscape`), `CircuitOptions` endurecidos y regiones `aria-live="polite"`. **Jerarquía funcional de la página principal, aprovechamiento del ancho completo en el Punto 1, fila colapsada en una sola línea, descarga global y segregación de histórico (`CMP-WEB-UI-001` v1.6.0, `design.md` v1.5.0)**:
  1. **Punto 1 — Calculadora de servicios y semáforo de rentabilidad a Ancho Completo (`100%`)** (`#escandallosHeading` / `.point-1-full-width`): Aprovecha el **100% del ancho de la pantalla** (`width: 100%; max-width: 100%; grid-column: 1 / -1;` en `.container` / `#workspaceMain` y en `.service-card-collapsible` / `.service-collapsed-row`). Muestra **exclusivamente los servicios activos** (`archived == false`), cada uno **colapsado por defecto en una sola línea horizontal** (`<details aria-expanded="false">` / `<summary class="service-collapsed-row">`): en **pantalla móvil (`< 768px`)** muestra únicamente **nombre, coste total, precio de venta y semáforo de rentabilidad**; en **pantallas más grandes (`>= 768px`)** mantiene una sola línea repartida en todo el ancho de la pantalla añadiendo **tiempo, personal y opciones de duplicar y archivar** (`.desktop-tablet-only`), ocultando sus productos hasta que el usuario despliegue la línea. Incluye en la cabecera las opciones de **descargar PDF (con resumen ejecutivo + desglose por servicio para Dirección) y XLSX de todos los servicios de un golpe**.
  2. **Punto 2 — Catálogo de costes del centro** (`#catalogHeading`).
  3. **Punto 3 — Impuestos, margen objetivo y equivalencias de dosis de cabina** (`#paramsHeading`).
  4. **Punto 4 — Histórico de servicios archivados y actualizaciones de precios** (`#historyHeading`): Al archivar un servicio activo se registra `archivedAtIso` (`ArchivedAtIso`), se retira del Punto 1 y se muestra en el Punto 4 **ordenado por fecha de archivado descendente** (`archivedAtIso DESC`) con sus productos colapsados y el botón *"Restaurar servicio"*.
  5. **Punto 5 — Diagnóstico técnico para soporte del sistema** (`#obsHeading`).
  6. **Cero nomenclatura interna o enumerados (`QR-UI-A11Y-001`)**: Prohibido mostrar códigos (`FR-*`, `QR-*`, `SEC-REQ-*`, `UC-*`, `BR-*`, `M1`–`M5`, `CMP-*`) o literales crudos (`OPTIMO`, `ALERTA`, `CRITICO`, `active`, `archived`, `none`, `canceled`). Verificación: `dotnet test --filter "FullyQualifiedName~WebUiComponentTests|FullyQualifiedName~WebAppEndToEndUiTests|FullyQualifiedName~WebUiInteractiveFlowsE2ETests"`.

---

## 3. Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-06 | agent-qa-engineer | Plan de tareas atómicas en .NET Puro con gobernanza y suites xUnit/BDD | CHG-001-ESCANDALLO-MVP |
| **1.1.0** | 2026-10-08 | agent-qa-engineer | Actualización del plan de tareas y suites BDD para arquitectura y seguridad v1.1.0 (.NET 10 LTS + Blazor) | CHG-001-ESCANDALLO-MVP |
| **1.2.0** | 2026-10-08 | agent-qa-engineer | Refuerzo en `TSK-007` de textos significativos de dominio en la UI y prohibición de mostrar códigos de requisitos o enumerados internos | CHG-001-ESCANDALLO-MVP |
| **1.3.0** | 2026-10-09 | agent-qa-engineer | Sincronización de `TSK-007` y suites en estado ROJO con `CMP-WEB-UI-001` v1.4.0 y `design.md` v1.3.0 (ordenación canónica Puntos 1–5, servicios activos colapsados ocultando productos en Punto 1 y servicios archivados en Punto 4 ordenados por fecha de archivado descendente) | CHG-001-ESCANDALLO-MVP |
| **1.4.0** | 2026-10-09 | agent-system-architect | Ajuste de `TSK-007` a `CMP-WEB-UI-001` v1.5.0: fila colapsada en una sola línea (móvil vs pantallas grandes) y descarga conjunta PDF/XLSX de todos los servicios de un golpe | CHG-001-ESCANDALLO-MVP |
| **1.5.0** | 2026-10-09 | agent-system-architect | Ajuste de `TSK-004` (`CMP-DOC-IO-001` v1.2.0: PDF con Resumen Ejecutivo inicial colapsado + listado de cada servicio desglosado para Dirección) y `TSK-007` (`CMP-WEB-UI-001` v1.6.0: aprovechamiento del 100% del ancho de pantalla en el Punto 1) | CHG-001-ESCANDALLO-MVP |
| **1.6.0** | 2026-10-09 | agent-qa-engineer | Diseño de pruebas BDD y xUnit en estado ROJO (FAILING) para `TSK-004` (informe PDF con `1. RESUMEN EJECUTIVO` colapsado y `2. DESGLOSE DETALLADO` por servicio activo para Dirección) y `TSK-007` (Punto 1 al 100% del ancho de pantalla `.point-1-full-width` y fila colapsada en una línea) | CHG-001-ESCANDALLO-MVP |
| **1.7.0** | 2026-10-09 | agent-developer | Implementación en VERDE (PASSING) de `TSK-004` (`RealPdfReportGenerator` + `EscandalloExportRoutes` con Resumen Ejecutivo colapsado y fichas desglosadas por servicio para Dirección) y `TSK-007` (Punto 1 al 100% del ancho de pantalla `.point-1-full-width` en Blazor, `index.html` y `app.css`) | CHG-001-ESCANDALLO-MVP |
