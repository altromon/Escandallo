# Informe de Validación Funcional Post-Desarrollo (Modo 2 — Downstream / Pre-PR)

> **Rol Actuante**: `agent-expert-user` (Expert User & Domain Evaluator)
> **Cambio Evaluado**: `CHG-001-ESCANDALLO-MVP` (`specs/changes/active/chg-001-escandallo-mvp/`)
> ** alcance Auditado**: Módulos de producción (`src/modules/*`, `src/index.ts`) y suites de verificación (`tests/unit/*.spec.ts`, `tests/features/*.feature`)
> **Fecha**: 2026-10-05
> **Veredicto Funcional**: **APROBADO SIN DESVIACIONES FUNCIONALES (100% CONFORME CON MVP Y MEJORAS M1–M5)**

---

## 1. Matriz de Conformidad por Caso de Uso (`UC-*`)

| Caso de Uso | Descripción | Módulo(s) Auditado(s) | Suite(s) BDD / Unitaria | Resultado de Validación Experta |
| :--- | :--- | :--- | :--- | :---: |
| **`UC-COST-CATALOG-001`** | Alta, Edición e Histórico del Catálogo de Costes y Parámetros por Centro de Estética | `src/modules/cost-engine/index.ts`, `src/modules/doc-io/index.ts`, `src/modules/iam/index.ts` | `cost-engine.spec.ts`, `doc-io-report.spec.ts`, `iam-tenant-rls.spec.ts` | **CONFORME** |
| **`UC-ESCANDALLO-CALC-001`** | Cálculo de Escandallo, Beneficios, PVP e Histórico por Servicio Estético | `src/modules/cost-engine/index.ts`, `src/modules/web-ui/index.ts` | `escandallo-calc.spec.ts`, `cascade-history.spec.ts`, `ui-a11y.spec.ts` | **CONFORME** |
| **`UC-EXEC-REPORT-001`** | Extracción de Informes Ejecutivos de Desglose de Costes y Beneficios | `src/modules/doc-io/index.ts` | `doc-io-report.spec.ts`, `fr-report-001.feature` | **CONFORME** |
| **`UC-SUBSCRIPTION-001`** | Contratación y Gestión de Suscripción Ilimitada por Centro mediante Stripe | `src/modules/billing/index.ts` | `billing-stripe.spec.ts`, `fr-billing-001.feature` | **CONFORME** |
| **`UC-OBSERVABILITY-001`** | Extracción de Logs, Trazas y Métricas del Sistema por Usuario Mantenedor | `src/modules/observability/index.ts` | `obs-telemetry.spec.ts`, `fr-obs-001.feature` | **CONFORME** |

---

## 2. Auditoría Detallada de Requisitos (`FR-*`, `QR-*`) y Fórmulas del Excel (`BR-CALC-FORMULAS-001`)

### 2.1 Fidelidad Matemática respecto a `Escandallo.xlsx` (`FR-COST-001`, `FR-CALC-001`)
Se ha verificado que `src/modules/cost-engine/index.ts` utiliza aritmética entera escalada en base 10 (`SCALE = 100_000_000n` con redondeo simétrico `HALF_SCALE`), reproduciendo al dígito los resultados del libro Excel original sin deriva de coma flotante IEEE 754:
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
- `applyCostMutationWithCascade` verifica que cuando un producto/coste **sí está vinculado** a escandallos activos, genera exactamente 1 entrada en histórico de costes, recalcula todos los escandallos afectados en la misma operación e incrementa la versión (`v1` $\rightarrow$ `v2`).
- Cuando el producto/coste **no está en uso por ningún escandallo** (`linkedEscandallos: []`), actualiza el precio con `costHistoryEntriesCreated: 0` y `recalculatedEscandallosCount: 0`, evitando ruido innecesario en la auditoría.

### 2.3 Informes Ejecutivos, Aislamiento Multi-Tenant, Suscripciones Stripe y Observabilidad (`FR-REPORT-001`, `FR-TENANT-001`, `FR-BILLING-001`, `FR-OBS-001`, `QR-UI-A11Y-001`, `QR-SEC-001`)
- **Informes Ejecutivos (`exportExecutiveReport`)**: Restringido estrictamente a `pdf` y `xlsx` (rechaza otros formatos con `400 UNSUPPORTED_EXPORT_FORMAT`), incluye las 9 columnas ejecutivas obligatorias y aplica bloqueo `402` si el centro supera la cuota gratuita sin suscripción activa.
- **Aislamiento Multi-Tenant (`authorizeCrossCenterCopy`, `verifySessionAndRls`)**: Bloquea cualquier intento de acceso o copia hacia/desde centros de otro propietario con `403`/`404` y evento `SECURITY` (`CROSS_TENANT_IDOR_ATTEMPT`).
- **Cuota Freemium y Stripe (`enforceFreemiumQuotaWithLock`, `verifyAndProcessStripeWebhook`)**: Permite crear hasta 2 escandallos activos gratis por centro y recalcularlos libremente; bloquea el 3º con `402 FREEMIUM_QUOTA_EXCEEDED`, serializa peticiones concurrentes (evitando que ráfagas paralelas superen el límite de 2) y verifica firmas HMAC-SHA256 con ventana de $300\text{ s}$ e idempotencia por `eventId`.
- **Observabilidad del Mantenedor (`setHotLogLevel`, `exportTelemetryBundle`, `redactTelemetryRecord`)**: Cambio de nivel a `DEBUG` en caliente sin reinicio exclusivo para `ACT-MAINTAINER-001` (`403` para usuarios normales), exportación en `ndjson`/`otlp_json`/`prometheus_csv` y ofuscación obligatoria `[REDACTED]` de `authorization`, `cookie` y `stripe_signature` junto con neutralización de saltos `\r\n` (anti-Log Injection).

---

## 3. Verificación de las 5 Mejoras Imprescindibles del MVP (`M1` a `M5`)

| Código | Mejora Acordada en Diseño | Implementación Verificada en `src/` | Cobertura en `tests/` | Estado |
| :---: | :--- | :--- | :--- | :---: |
| **`M1`** | **Archivado de Escandallos (desbloqueo de cuota sin pérdida de datos) y Baja Lógica de Costes** | `archiveCostOrService` en `src/modules/cost-engine/index.ts` bloquea el borrado físico de recursos con uso/histórico (`409 PHYSICAL_DELETE_FORBIDDEN`) aplicando estado `archived`; `enforceFreemiumQuotaWithLock` en `src/modules/billing/index.ts` permite `archivar_escandallo` en estado `canceled` y desbloquea la edición (`200 OK`) al quedar $\le 2$ activos. | `cascade-history.spec.ts`, `billing-stripe.spec.ts` | **VERIFICADO** |
| **`M2`** | **Doble PVP (`PVP Calculado` vs `PVP Comercial Fijado`) y Semáforo de Erosión de Margen (`🟢/🟡/🔴`)** | `evaluateMarginSemaphore` en `src/modules/cost-engine/index.ts` calcula el `realPriceExclVatEur` y `realNetProfitEur` frente al `fixedCommercialPvpEur` y clasifica determinísticamente en `OPTIMO`, `ALERTA` y `CRITICO` (incluyendo la frontera exacta `0.00` como `CRITICO`). | `escandallo-calc.spec.ts` | **VERIFICADO** |
| **`M3`** | **Constructor Dinámico de Receta (sin tabla rígida de 30 columnas) y Adaptabilidad Móvil/Tablet (`portrait`/`landscape`)** | `calculateEscandalloService` acepta un array dinámico `productLines` (de 1 a 100 líneas con guarda `MAX_RECIPE_LINES_EXCEEDED`); `validateResponsiveViewportLayout` y `evaluateDynamicRecipeBuilderA11y` en `src/modules/web-ui/index.ts` validan `stack-cards` en móvil, `hybrid-2col` en tablet y `split-pane` en PC sin scroll horizontal y áreas táctiles $\ge 44\text{ px}$. | `escandallo-calc.spec.ts`, `ui-a11y.spec.ts` | **VERIFICADO** |
| **`M4`** | **Plantilla Semilla, Importación Segura `.xlsx` y Copia de Catálogo / Duplicación entre Centros Propios** | `inspectAndParseExcelCatalog` y `sanitizeSpreadsheetCell` en `src/modules/doc-io/index.ts` validan límites anti-ZipBomb/XXE y neutralizan fórmulas DDE (`'=cmd...`); `authorizeCrossCenterCopy` en `src/modules/iam/index.ts` permite copiar catálogos entre centros del mismo propietario hasta 5.000 ítems. | `doc-io-report.spec.ts`, `iam-tenant-rls.spec.ts` | **VERIFICADO** |
| **`M5`** | **Unidades de Cabina (`ml`, `g`, `ud`), Dosis Fraccionaria y `% Merma` Configurable** | `calculateProductUnitCost` y `sumRecipeLinesScaled` en `src/modules/cost-engine/index.ts` soportan unidades `ml`, `g`, `ud`, validan `wastagePct` en $[0, 1]$ (`WASTAGE_OUT_OF_RANGE`) y aplican el factor $(1 + \text{merma})$ con precisión de 8 decimales. | `cost-engine.spec.ts`, `escandallo-calc.spec.ts` | **VERIFICADO** |

---

## 4. Cuestiones Abiertas para el Product Owner (`open-questions`)

- **Ninguna desviación funcional bloqueante detectada (`0` hallazgos de regresión o deriva de alcance)**.
- El incremento `CHG-001-ESCANDALLO-MVP` está listo para someterse a la **Tríada de Auditoría Pre-Merge** (`agent-security-auditor`, `agent-code-reviewer` y `agent-compliance-checker`).
