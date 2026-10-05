# Informe de Validación Funcional Post-Desarrollo (Modo 2 — Downstream / Pre-PR)

> **Rol Actuante**: `agent-expert-user` (Expert User & Domain Evaluator)
> **Cambio Evaluado**: `CHG-001-ESCANDALLO-MVP` (`specs/changes/active/chg-001-escandallo-mvp/`)
> **Stack Evaluado**: **C# 12 / .NET 8.0 (`Escandallo.sln`)** con `System.Decimal` de 128 bits (`MidpointRounding.AwayFromZero`) y `xUnit`
> **Alcance Auditado**: Módulos de producción (`src/Escandallo.Core/Modules/*`) y suites de verificación (`tests/Escandallo.UnitTests/*Tests.cs`, `tests/features/*.feature`)
> **Fecha**: 2026-10-05
> **Veredicto Funcional**: **APROBADO SIN DESVIACIONES FUNCIONALES (100% CONFORME CON MVP Y MEJORAS M1–M5)**

---

## 1. Matriz de Conformidad por Caso de Uso (`UC-*`)

| Caso de Uso | Descripción | Módulo(s) C# Auditado(s) | Suite(s) xUnit / BDD | Resultado de Validación Experta |
| :--- | :--- | :--- | :--- | :---: |
| **`UC-COST-CATALOG-001`** | Alta, Edición e Histórico del Catálogo de Costes y Parámetros por Centro de Estética | `CostCatalogCalculator.cs`, `DocumentIoService.cs`, `IamTenantGuard.cs` | `CostEngineTests.cs`, `DocIoReportTests.cs`, `IamTenantRlsTests.cs` | **CONFORME** |
| **`UC-ESCANDALLO-CALC-001`** | Cálculo de Escandallo, Beneficios, PVP e Histórico por Servicio Estético | `EscandalloCalculator.cs`, `CascadeHistoryService.cs`, `AccessibilityValidator.cs` | `EscandalloCalcTests.cs`, `CascadeHistoryTests.cs`, `UiA11yTests.cs` | **CONFORME** |
| **`UC-EXEC-REPORT-001`** | Extracción de Informes Ejecutivos de Desglose de Costes y Beneficios | `DocumentIoService.cs` | `DocIoReportTests.cs`, `fr-report-001.feature` | **CONFORME** |
| **`UC-SUBSCRIPTION-001`** | Contratación y Gestión de Suscripción Ilimitada por Centro mediante Stripe | `BillingQuotaService.cs` | `BillingStripeTests.cs`, `fr-billing-001.feature` | **CONFORME** |
| **`UC-OBSERVABILITY-001`** | Extracción de Logs, Trazas y Métricas del Sistema por Usuario Mantenedor | `TelemetryService.cs` | `ObsTelemetryTests.cs`, `fr-obs-001.feature` | **CONFORME** |

---

## 2. Auditoría Detallada de Requisitos (`FR-*`, `QR-*`) y Fórmulas del Excel (`BR-CALC-FORMULAS-001`)

### 2.1 Fidelidad Matemática respecto a `Escandallo.xlsx` (`FR-COST-001`, `FR-CALC-001`)
Se ha verificado que `src/Escandallo.Core/Modules/CostEngine/CostCatalogCalculator.cs` y `src/Escandallo.Core/Modules/CostEngine/EscandalloCalculator.cs` utilizan el tipo nativo de 128 bits en base 10 de C# (`decimal` / `System.Decimal`) con redondeo determinista `Math.Round(val, 8, MidpointRounding.AwayFromZero)`, reproduciendo al dígito los resultados del libro Excel original sin deriva de coma flotante IEEE 754:
- **Gastos Generales (`Tabla1`)**: `335.00 + 100.00 + 440.00 = 875.00 EUR/mes` $\rightarrow$ dividido entre `10560 min/mes` = **`0.08285985 EUR/min`**.
- **Productos (`Tabla2`)**:
  - *Cleasing Milk* (`34.51 EUR / 500 ml`, merma `0%`) = **`0.06902 EUR/ml`**.
  - *Colagen Mask* (`86.52 EUR / 12 ud`, merma `0%`) = **`7.21 EUR/ud`**.
  - *Ácido Glicólico* (`50.00 EUR / 250 g`, merma `5%`) = coste base **`0.20 EUR/g`**, coste efectivo con merma **`0.21 EUR/g`**.
- **Personal (`Tabla3` / `Tabla4`)**:
  - *Técnico* (`1100 EUR`, retención `0.20`, coef. `1.5`, `10560 min`) = **`1980.00 EUR/mes`** (**`0.1875 EUR/min`**).
  - *Enfermero* (`1800 EUR`, retención `0.22`, coef. `1.5`, `10560 min`) = **`3294.00 EUR/mes`** (**`0.31193182 EUR/min`**).
  - *Doctor* (`3000 EUR`, retención `0.25`, coef. `1.5`, `10560 min`) = **`5625.00 EUR/mes`** (**`0.53267045 EUR/min`**).
- **Escandallo Servicio *Higiene Facial* (`Tabla5` + `Tabla57`, 60 min, Técnico, 10 ml Cleasing Milk + 1 ud Colagen Mask, Beneficio `50%`, IVA `21%`)**:
  - Gastos Generales imputados: **`4.971591 EUR`**
  - Coste Persona: **`11.25 EUR`**
  - Coste Productos: **`7.9002 EUR`**
  - Escandallo Total: **`24.121791 EUR`**
  - Beneficio Neto: **`12.0608955 EUR`**
  - Precio con Beneficio (Base Imponible sin IVA): **`36.1826865 EUR`**
  - PVP Calculado con IVA: **`43.78105067 EUR`**

### 2.2 Recálculo en Cascada e Histórico Condicional (`FR-HIST-001`, `BR-RECALC-HISTORY-001`)
- `CascadeHistoryService.ApplyCostMutationWithCascade` verifica que cuando un producto/coste **sí está vinculado** a escandallos activos, genera exactamente 1 entrada en histórico de costes, recalcula todos los escandallos afectados en la misma operación e incrementa la versión (`v1` $\rightarrow$ `v2`).
- Cuando el producto/coste **no está en uso por ningún escandallo** (`LinkedEscandallos: []`), actualiza el precio con `CostHistoryEntriesCreated: 0` y `RecalculatedEscandallosCount: 0`, evitando ruido innecesario en la auditoría.

### 2.3 Informes Ejecutivos, Aislamiento Multi-Tenant, Suscripciones Stripe y Observabilidad (`FR-REPORT-001`, `FR-TENANT-001`, `FR-BILLING-001`, `FR-OBS-001`, `QR-UI-A11Y-001`, `QR-SEC-001`)
- **Informes Ejecutivos (`DocumentIoService.ExportExecutiveReport`)**: Restringido estrictamente a `pdf` y `xlsx` (rechaza otros formatos con `400 UNSUPPORTED_EXPORT_FORMAT`), incluye las 9 columnas ejecutivas obligatorias y aplica bloqueo `402` si el centro supera la cuota gratuita sin suscripción activa.
- **Aislamiento Multi-Tenant (`IamTenantGuard.AuthorizeCrossCenterCopy`, `IamTenantGuard.VerifySessionAndRls`)**: Bloquea cualquier intento de acceso o copia hacia/desde centros de otro propietario con `403`/`404` y evento `SECURITY` (`CROSS_TENANT_IDOR_ATTEMPT`).
- **Cuota Freemium y Stripe (`BillingQuotaService.EnforceFreemiumQuotaWithLock`, `BillingQuotaService.VerifyAndProcessStripeWebhook`)**: Permite crear hasta 2 escandallos activos gratis por centro y recalcularlos libremente; bloquea el 3º con `402 FREEMIUM_QUOTA_EXCEEDED`, serializa peticiones concurrentes (evitando que ráfagas paralelas superen el límite de 2) y verifica firmas HMAC-SHA256 con ventana de $300\text{ s}$ e idempotencia por `EventId`.
- **Observabilidad del Mantenedor (`TelemetryService.SetHotLogLevel`, `TelemetryService.ExportTelemetryBundle`, `TelemetryService.RedactTelemetryRecord`)**: Cambio de nivel a `DEBUG` en caliente sin reinicio exclusivo para `ACT-MAINTAINER-001` (`403` para usuarios normales), exportación en `ndjson`/`otlp_json`/`prometheus_csv` y ofuscación obligatoria `[REDACTED]` de `authorization`, `cookie` y `stripe_signature` junto con neutralización de saltos `\r\n` (anti-Log Injection).

---

## 3. Verificación de las 5 Mejoras Imprescindibles del MVP (`M1` a `M5`)

| Código | Mejora Acordada en Diseño | Implementación Verificada en `src/Escandallo.Core/` | Cobertura en `tests/Escandallo.UnitTests/` | Estado |
| :---: | :--- | :--- | :--- | :---: |
| **`M1`** | **Archivado de Escandallos (desbloqueo de cuota sin pérdida de datos) y Baja Lógica de Costes** | `CascadeHistoryService.ArchiveCostOrService` bloquea el borrado físico de recursos con uso/histórico (`409 PHYSICAL_DELETE_FORBIDDEN`) aplicando estado `archived`; `BillingQuotaService.EnforceFreemiumQuotaWithLock` permite `archivar_escandallo` en estado `canceled` y desbloquea la edición (`200 OK`) al quedar $\le 2$ activos. | `CascadeHistoryTests.cs`, `BillingStripeTests.cs` | **VERIFICADO** |
| **`M2`** | **Doble PVP (`PVP Calculado` vs `PVP Comercial Fijado`) y Semáforo de Erosión de Margen (`🟢/🟡/🔴`)** | `EscandalloCalculator.EvaluateMarginSemaphore` calcula el `RealPriceExclVatEur` y `RealNetProfitEur` frente al `FixedCommercialPvpEur` y clasifica determinísticamente en `OPTIMO`, `ALERTA` y `CRITICO` (incluyendo la frontera exacta `0.00` como `CRITICO`). | `EscandalloCalcTests.cs` | **VERIFICADO** |
| **`M3`** | **Constructor Dinámico de Receta (sin tabla rígida de 30 columnas) y Adaptabilidad Móvil/Tablet (`portrait`/`landscape`)** | `EscandalloCalculator.CalculateEscandalloService` acepta una lista dinámica `ProductLines` (de 1 a 100 líneas con guarda `MAX_RECIPE_LINES_EXCEEDED`); `AccessibilityValidator.ValidateResponsiveViewportLayout` y `EvaluateDynamicRecipeBuilderA11y` validan `stack-cards` en móvil, `hybrid-2col` en tablet y `split-pane` en PC sin scroll horizontal y áreas táctiles $\ge 44\text{ px}$. | `EscandalloCalcTests.cs`, `UiA11yTests.cs` | **VERIFICADO** |
| **`M4`** | **Plantilla Semilla, Importación Segura `.xlsx` y Copia de Catálogo / Duplicación entre Centros Propios** | `DocumentIoService.InspectAndParseExcelCatalog` y `SanitizeSpreadsheetCell` validan límites anti-ZipBomb/XXE y neutralizan fórmulas DDE (`'=cmd...`); `IamTenantGuard.AuthorizeCrossCenterCopy` permite copiar catálogos entre centros del mismo propietario hasta 5.000 ítems. | `DocIoReportTests.cs`, `IamTenantRlsTests.cs` | **VERIFICADO** |
| **`M5`** | **Unidades de Cabina (`ml`, `g`, `ud`), Dosis Fraccionaria y `% Merma` Configurable** | `CostCatalogCalculator.CalculateProductUnitCost` y `EscandalloCalculator.SumRecipeLines` soportan unidades `ml`, `g`, `ud`, validan `WastagePct` en $[0, 1]$ (`WASTAGE_OUT_OF_RANGE`) y aplican el factor $(1 + \text{merma})$ con precisión de 8 decimales en `System.Decimal`. | `CostEngineTests.cs`, `EscandalloCalcTests.cs` | **VERIFICADO** |

---

## 4. Cuestiones Abiertas para el Product Owner (`open-questions`)

- **Ninguna desviación funcional bloqueante detectada (`0` hallazgos de regresión o deriva de alcance)**.
- El incremento `CHG-001-ESCANDALLO-MVP` en **C# (`.NET 8.0`)** supera todos los tests (`28/28` en `Escandallo.UnitTests.dll`) y la **Tríada de Auditoría Pre-Merge**.
