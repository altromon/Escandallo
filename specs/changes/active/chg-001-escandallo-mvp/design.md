---
id: DSG-CHG-001-ESCANDALLO-MVP
type: spec-design
change-id: CHG-001-ESCANDALLO-MVP
title: "Diseño Técnico: MVP Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética (.NET 10 LTS + Blazor)"
version: "1.5.0"
schema-version: "1.0"
handoff: "HOF-CHG-001-ESCANDALLO-MVP"
architecture-component: "CMP-ESCANDALLO-SYS-001"
enclave: "SEC-ENC-APP-CORE-001"
status: draft
citations:
  - id: FR-AUTH-001
    digest: sha256:091609e9caf0022c40a0c719834ed24b0925d59ac0dac36be6798d65cd044921
    comment: Citación de requirement (Registro de Cuenta de Usuario, Inicio de Sesión Obligatorio y Gestión de Centros por Titular)
  - id: UC-AUTH-001
    digest: sha256:2c8bb7f497f595bb308ccbef4cd478138cef23b7c3eec32a7eefdd8f23422b08
    comment: Citación de use-case (Registro de Cuenta de Cliente, Inicio de Sesión y Gestión de Centros Propios)
  - id: FR-TENANT-001
    digest: sha256:e3577d1867a8952695b50d01860c62aff01423fa7a2d87e24e23f8f3fb5fe544
    comment: Citación de requirement (Aislamiento Multi-Tenant de Centros de Estética por Cliente)
  - id: FR-COST-001
    digest: sha256:cc4dca5ec5575f5fbd05088d08693e4053b1bc4f8be09befd4e8303f0cdcc4e6
    comment: Citación de requirement (Alta y Cálculo Unitario de Productos, Personal, Gastos Generales y Parámetros)
  - id: FR-CALC-001
    digest: sha256:b6bac635b637c67d0b8328f03f03f7264fdd58fe5725e26f8495f04205d3f140
    comment: Citación de requirement (Cálculo de Escandallo Total, Beneficio y PVP de Servicios Estéticos)
  - id: FR-HIST-001
    digest: sha256:e3650f8275f8280beeed12683fd26b226b94a2847502680dc45ea0954412e68d
    comment: Citación de requirement (Registro Histórico y Recálculo Automático en Cascada de Escandallos)
  - id: FR-REPORT-001
    digest: sha256:68013a94bc1a7bf11ade7cfeb179eeacfb2e76df83a7c6babb87c3680947addc
    comment: Citación de requirement (Generación y Exportación de Informes Ejecutivos de Costes y Beneficios)
  - id: FR-BILLING-001
    digest: sha256:b300114e2a6a96c0e56e2208bde40f9e34fac96b3e53f16e9c14337e1ee93618
    comment: Citación de requirement (Control de Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro con Stripe)
  - id: FR-OBS-001
    digest: sha256:deb4e802bacd5796860130d1a0be0a4ec929fa97d8a198722462c4a34a27f662
    comment: Citación de requirement (Extracción de Logs, Trazas y Métricas del Servidor para Usuario Mantenedor)
  - id: QR-UI-A11Y-001
    digest: sha256:70285a48a37f17834b7b65b8f0fbbacfee815d0a3ecaad111a0d3a4aa8bd1ca7
    comment: Citación de requirement (Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil)
  - id: QR-SEC-001
    digest: sha256:0ca840c300d2eec4fc750116e336814d0913cfe09e1c5e4a97ca2be4d4816bdc
    comment: Citación de requirement (Seguridad de Servidor frente a Ciberataques, Protección IDOR y Verificación de Webhooks)
  - id: SEC-REQ-TENANT-001
    digest: sha256:b967c202bb20317ee6832c43f38b5ca8c96767ddfb89bdb8fd336fb3c6fd853c
    comment: Citación de security-requirement (Autorización Multi-Tenant Zero-Trust, Prevención IDOR/BOLA y Row-Level Security)
  - id: SEC-REQ-BILLING-001
    digest: sha256:47d2d8d09fe12a5842d0b9a2ee5bec9449547e697a7687fefde3716aec6a7f3e
    comment: Citación de security-requirement (Control Atómico Anti-Race-Condition de Cuota Freemium y Verificación HMAC de Webhooks Stripe)
  - id: SEC-REQ-XLSX-001
    digest: sha256:1f33f716455bb248ccdcddcb6a50d2ec2411ee34c578752fdee031ee59126ed8
    comment: Citación de security-requirement (Parseo Seguro de Plantillas Excel Anti-XXE/ZipBomb y Neutralización de Fórmulas DDE)
  - id: SEC-REQ-DOS-001
    digest: sha256:55e9d199b39728a7c3507c0d54f32fc8d990a640c1817a4d214dca27c7500aa9
    comment: Citación de security-requirement (Rate Limiting por Inquilino, Cuotas de Concurrencia en Exportación PDF/Excel y Límites de Recálculo)
  - id: SEC-REQ-OBS-001
    digest: sha256:80be1497c251a8e8d32bdb5d6e16b4f92805c5d94d66a0d1d47605ca41b3a9df
    comment: Citación de security-requirement (RBAC Estricto para Mantenedor, Redactado Automático PII/Secretos en Logs DEBUG y Anti-Log-Injection)
  - id: UC-COST-CATALOG-001
    digest: sha256:8d8c47f4590854050c09745a54ccffab0202a503519a696f0a3f66bc95b80cda
    comment: Citación de use-case (Alta, Edición e Histórico del Catálogo de Costes y Parámetros por Centro de Estética)
  - id: UC-ESCANDALLO-CALC-001
    digest: sha256:64eb7c2134a2edfe182d7e66dd249715fcb3e7fb099bb73e596197d47b53e817
    comment: Citación de use-case (Cálculo de Escandallo, Beneficios, PVP e Histórico por Servicio Estético)
  - id: UC-EXEC-REPORT-001
    digest: sha256:cb7aa5553d560850c52daf2b88919dc678a95007deb35ddd273fdcb7fe226744
    comment: Citación de use-case (Extracción de Informes Ejecutivos de Desglose de Costes y Beneficios)
  - id: UC-SUBSCRIPTION-001
    digest: sha256:45b6e45086ee23127cc6177941141ca0f53cb8e3ea9238a3d5bede42643ddfdf
    comment: Citación de use-case (Contratación y Gestión de Suscripción Ilimitada por Centro mediante Stripe)
  - id: UC-OBSERVABILITY-001
    digest: sha256:ba8624fb3f8e2f6cd449ed466f8409698d383d07b2c127b27e753ab514de300e
    comment: Citación de use-case (Extracción de Logs, Trazas y Métricas del Sistema por Usuario Mantenedor)
  - id: BR-CALC-FORMULAS-001
    digest: sha256:06a57f45d3c9c22d6a0dd92920b927738c548fe27d6dc08d487462c84d9e2a5f
    comment: Citación de business-rule (Fórmulas Matemáticas Canónicas de Costes, Escandallo, Beneficio y PVP)
  - id: BR-RECALC-HISTORY-001
    digest: sha256:5e2dd257bc2d3b4d238ddf8bf6426936dfed4d36bae4c36349e69b9bba32b973
    comment: Citación de business-rule (Inmutabilidad del Histórico y Recálculo en Cascada de Escandallos)
  - id: BR-TENANT-ISOLATION-001
    digest: sha256:43ea9c7a0aa1b9e45c4b6c9391328c05e711bb21b02b93d41c700f09e5b29927
    comment: Citación de business-rule (Aislamiento Multi-Tenant Estricto por Cliente y Centro de Estética)
  - id: BR-FREEMIUM-QUOTA-001
    digest: sha256:a8f31eee3ded62f4e1ad3fd4049daf41483a84d7d28c7b9283b9fdd1f7029754
    comment: Citación de business-rule (Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro en Stripe)
  - id: ABUSE-IDOR-TENANT-001
    digest: sha256:3a99f135074f673079558d58e5d3b831e39e37041cebc40fbdc38d9eda4f0e38
    comment: Citación de abuse-case (Exfiltración Cross-Tenant de Costes, Catálogos e Informes mediante IDOR/BOLA)
  - id: ABUSE-QUOTA-BYPASS-001
    digest: sha256:bb29035a386d1929c993ba477f90f5360322729452c5e1fb62cdca81e7fa86f3
    comment: Citación de abuse-case (Evasión de Cuota Freemium mediante Condiciones de Carrera y Spoofing de Webhooks Stripe)
  - id: ABUSE-XLSX-INJECT-001
    digest: sha256:bf41cf49f3b2bfcef28c3659e60c3455b91c3f90559988917bf40705ab3414d4
    comment: Citación de abuse-case (Inyección de Fórmulas DDE, XXE y Bombas ZIP en Importación y Exportación de Excel)
  - id: ABUSE-DOS-CASCADE-001
    digest: sha256:0ddb1035384cf542230403e359bba7b47bf1a1ca918bbfcb87ea32823b7c9338
    comment: Citación de abuse-case (Denegación de Servicio Asimétrica por Tormenta de Recálculos en Cascada y Renderizado PDF)
  - id: ABUSE-PRIV-OBS-001
    digest: sha256:8931ad1975e1895bd4cce0f2332704f458015f53dac39c81fb17f37713d52878
    comment: Citación de abuse-case (Escalada de Privilegios al Rol Mantenedor, Log Forgery y Fuga de Secretos en Modo DEBUG)
  - id: CMP-ESCANDALLO-SYS-001
    digest: sha256:b5154d68fbd66d7de78e8549f8beaf97a29a042a8a1d079538820e5ce0b24f2e
    comment: 'Citación de component (Sistema Raíz: Plataforma SaaS de Escandallos para Centros de Estética (.NET 10 LTS + Blazor))'
  - id: CMP-WEB-UI-001
    digest: sha256:9e047408aef283aff6a2f98ccd699fc9409a9ff7b24d3058daf6b5db1b954639
    comment: Citación de component (Interfaz Web Adaptativa en ASP.NET Core Blazor Web App (WCAG 2.1 AA))
  - id: CMP-IAM-TENANT-001
    digest: sha256:e994c43bcbf683083548aabebaece6f03cfe6887546fdcad48ab52367aa1a5fd
    comment: Citación de component (Guardia de Identidad, Aislamiento Multi-Tenant (RLS) y Limitador Anti-DoS)
  - id: CMP-COST-ENGINE-001
    digest: sha256:de887bfa9e3e74c060cf4ef1ff3f7ff95db6777476e7295b350e5ecbe049fb63
    comment: Citación de component (Motor de Dominio de Costes en System.Decimal (128-bit) y Recálculo Transaccional en Cascada)
  - id: CMP-DOC-IO-001
    digest: sha256:9afdf33f993257e2577d9ee8389f7c2788985b491f6950f6b1fcdb83f2b7499e
    comment: Citación de component (Motor Seguro de Importación de Catálogos (.xlsx) y Exportación Ejecutiva (.pdf / .xlsx))
  - id: CMP-BILLING-STRIPE-001
    digest: sha256:f9e571f8d4adfa299df3a8e7fd2fc7ab513e6359cc21b4eada64cbcfedbd0aac
    comment: Citación de component (Gestor Atómico de Cuota Freemium y Pasarela Idempotente de Suscripciones Stripe)
  - id: CMP-OBS-TELEMETRY-001
    digest: sha256:1cde03f1f44ed3b07a9d91b7beffb50562317e26cb7527c257f5d44d90154829
    comment: Citación de component (Subsistema de Observabilidad Dual en Caliente, Redacción PII ([REDACTED]) y Exportación)
  - id: ADR-001-COST-EFFECTIVE-DEPLOYMENT
    digest: sha256:4aea90435f34aeb4da963c5e84c96dbce9261c28573d1ce8041c12e52d818fea
    comment: 'Citación de architecture-decision-record (Topología de Despliegue Ultra-Rentable: Cloudflare Edge WAF + Contenedores .NET 10 LTS y PostgreSQL 16 en Cloud VPS Europeo)'
  - id: ADR-002-MODULAR-MONOLITH-STACK
    digest: sha256:94c5200f67cc9a60e75f68d501de681cd954586fb11e0504edf0427df350cb15
    comment: Citación de architecture-decision-record (Adopción de Monolito Modular Hexagonal en .NET 10 LTS con UI Unificada en Blazor Web App)
  - id: ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA
    digest: sha256:7a0386999c2dfd4b99c33476456391c49ff4ffb075519dd8f35f1ed4189ba62b
    comment: Citación de architecture-decision-record (Aislamiento Multi-Tenant con PostgreSQL FORCE RLS y Control Atómico de Cuota Freemium con SELECT FOR UPDATE)
  - id: ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE
    digest: sha256:6b5430df489834f697b516d9e3439df5b1aa0e967748acc027e88e10c014ff64
    comment: Citación de architecture-decision-record (Aritmética Financiera Exacta de 128 Bits (System.Decimal / NUMERIC 18,6) y Motor Transaccional Síncrono de Cascada)
  - id: ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS
    digest: sha256:cf98625fb1f5dfbdbc57ec63bc6edaa96292ac77c2c75893a68c1807d560f742
    comment: Citación de architecture-decision-record (Observabilidad Dual en Caliente con Redacción Inmutable (Serilog + OpenTelemetry) y Motor Documental 100% MIT (ClosedXML + PDFsharp))
---

# Diseño Técnico: CHG-001-ESCANDALLO-MVP

## 1. Mapeo Arquitectónico y Enclave Zero Trust

El cambio materializa el sistema raíz `CMP-ESCANDALLO-SYS-001` y sus seis subsistemas de Nivel 2 (`CMP-WEB-UI-001`, `CMP-IAM-TENANT-001`, `CMP-COST-ENGINE-001`, `CMP-DOC-IO-001`, `CMP-BILLING-STRIPE-001`, `CMP-OBS-TELEMETRY-001`) como un **Monolito Modular Hexagonal en .NET 10 LTS (C# 14, ASP.NET Core Minimal APIs + Blazor Web App)** desplegado en `SEC-ENC-APP-CORE-001`, protegido perimetralmente por `SEC-ENC-EDGE-WAF-001` (Cloudflare WAF + Caddy mTLS con `KnownProxies`) y respaldado por PostgreSQL 16 con `FORCE ROW LEVEL SECURITY` y rol `NOBYPASSRLS` en `SEC-ENC-DATA-OBS-001`.

## 2. Contratos de Datos e Interfaces (.NET 10 LTS Domain Models & Services)

- **`ICostCatalogService` (`CMP-COST-ENGINE-001`)**:
  - `CalculateProductUnitCost(decimal purchasePrice, decimal packageQuantity, decimal wastePct)` $\rightarrow$ `decimal` (`System.Decimal` 128-bit, `MidpointRounding.AwayFromZero`, con cotas anti-`OverflowException`: `PurchasePrice <= 1_000_000m`, `UnitsPerPackage >= 0.0001m`, `WastePct in [0m, 99.99m]`).
  - `CalculateStaffMinuteCost(decimal netSalary, decimal retentionPct, decimal companyCoeff, int monthlyMinutes)` $\rightarrow$ `(decimal CompanyCost, decimal MinuteCost)`.
  - `CalculateOverheadMinuteRate(IReadOnlyList<OverheadItem> items, int monthlyMinutes)` $\rightarrow$ `decimal`.
- **`IEscandalloCalculatorService` (`CMP-COST-ENGINE-001`)**:
  - `CalculateServiceEscandallo(EscandalloInput input)` $\rightarrow$ `EscandalloCalculationResult` (`OverheadAllocated`, `StaffCost`, `ProductsCost`, `TotalCost`, `NetProfitEur`, `PriceExVat`, `CalculatedPvp`, `CommercialPvp`, `SemaphoreStatus`, máximo `100` líneas de coste por servicio M3).
  - `PreviewCostUpdateImpact(Guid centerId, Guid costId, decimal newAmount)` $\rightarrow$ `CascadeImpactPreview` (M3).
  - `UpdateCostWithCascade(Guid centerId, Guid costId, decimal newAmount)` $\rightarrow$ `CascadeUpdateResult` (persiste histórico inmutable `cost_history` solo si `LinkedEscandallosCount >= 1`, `BR-RECALC-HISTORY-001`).
- **`ITenantIsolationGuard` (`CMP-IAM-TENANT-001`)**:
  - `AuthorizeCenterAccess(string userId, Guid centerId, string action)` y `AuthorizeDualCenterCopy(string userId, Guid sourceCenterId, Guid targetCenterId)` (M4, `SEC-REQ-TENANT-001`), revalidando `AuthenticationStateProvider` en cada evento Blazor y ejecutando `SELECT set_config('app.current_center_id', @centerId::text, true)` (`SET LOCAL`) en cada transacción `Npgsql`.
- **`IDocumentIoService` (`CMP-DOC-IO-001`)**:
  - `InspectAndImportCatalogXlsx(XlsxImportEnvelope envelope)` (pre-inspección ZIP `<= 2MB` comprimido, stream contador `BoundedReadStream` `<= 10MB` reales descomprimidos sobre `RecyclableMemoryStream`, ratio `<= 20:1`, `<= 5000` filas, `DtdProcessing.Prohibit`, `XmlResolver = null`, escape DDE con `'` tras `TrimStart()` y `XLDataType.Text` en `ClosedXML`, `SEC-REQ-XLSX-001`).
  - `ExportExecutiveReport(ReportExportRequest request)` y `RealPdfReportGenerator.BuildExecutiveCenterPdfBytes(TenantCatalogDto tenant)` (exclusivamente `pdf` con `PDFsharp` y `xlsx` con `ClosedXML`, acotado por `SemaphoreSlim(2, 2)` y `<= 10` exportaciones/min por centro, `FR-REPORT-001`, `SEC-REQ-DOS-001`).
  - **Maquetación Ejecutiva del PDF para Dirección (`RealPdfReportGenerator`)**:
    El PDF descargado consolida todos los servicios activos del centro (`archived == false`) con formato visual estructurado para Dirección (tipografía jerárquica `Helvetica-Bold`/`Helvetica`, separadores, tablas alineadas y textos ejecutivos sin enumerados crudos):
    1. **Resumen Ejecutivo al Principio (Tabla Consolidada de Servicios con la Información Colapsada)**: Cabecera del centro, parámetros globales (`% IVA`, `% Beneficio Objetivo`, `% Merma`) y tabla resumen que lista todos los servicios activos con los campos de la vista colapsada (**Servicio**, **Duración en min**, **Personal asignado**, **Coste Total €**, **Precio de Venta €**, **Precio Recomendado €** y **Estado de Rentabilidad** en lenguaje claro).
    2. **Listado de Cada Servicio Desglosado para Presentar a Dirección**: A continuación del resumen ejecutivo, incluye una ficha detallada por cada servicio activo con su desglose de costes (**Productos con merma €/%**, **Personal imputado €/%**, **Gastos Generales Fijos €/%**, **Escandallo Total €**), análisis de rentabilidad (**Beneficio Neto Objetivo €**, **Precio Base sin IVA €**, **PVP Recomendado €** vs. **PVP Comercial Fijado €**, **Beneficio Neto Real €** y **% Margen Neto Real**) y el **listado desglosado de los productos cosméticos de la receta** (nombre del producto, dosis en ml, envase/precio de compra y coste unitario imputado con merma), con paginación automática en A4.
- **`IBillingQuotaService` (`CMP-BILLING-STRIPE-001`)**:
  - `AuthorizeEscandalloOperation(QuotaCheckRequest request)` (cuota gratuita de 2 escandallos `active` con bloqueo `SELECT ... FOR UPDATE`, validación de `metadata.center_id` en Checkout y bloqueo/desbloqueo post-cancelación M1).
  - `ProcessStripeWebhook(StripeWebhookEnvelope webhook)` (`Stripe.EventUtility.ConstructEvent` sobre stream crudo $\le 64\text{ KB}$, HMAC-SHA256 $\le 300\text{ s}$ e idempotencia monótona por `EventId`, `SEC-REQ-BILLING-001`).
- **`IObservabilityService` (`CMP-OBS-TELEMETRY-001`)**:
  - `SetHotLogLevel(string actorRole, string newLevel)` y `ExportTelemetry(TelemetryExportRequest request)` con `Destructuring.ToMaximumDepth(3)`, redactado inmutable `PiiAndSecretRedactingPolicy` (`[REDACTED]`, incluyendo `Exception.Message`) en todos los niveles incluido `DEBUG` y canal protegido para eventos `SECURITY` (`SEC-REQ-OBS-001`).
- **`IWebUiComponentRenderer` y Componentes Razor (`CMP-WEB-UI-001`)**:
  - **Regla Transversal de Textos de Usuario (Cero Jerga Interna ni Enumerados)**: Todos los textos visibles en el DOM renderizado deben ser significativos y naturales para el usuario del centro de estética. Queda **estrictamente prohibido** renderizar en la interfaz identificadores de requisitos o artefactos (`FR-*`, `QR-*`, `SEC-REQ-*`, `UC-*`, `BR-*`, `M1`–`M5`, `CMP-*`), nombres internos de campos o valores crudos de enumerados (`OPTIMO`, `ALERTA`, `CRITICO`, `active`, `archived`, `none`, `canceled`, `producto`, `personal`, `gasto_general`).
  - **Ordenación Canónica de la Página Principal**:
    1. **Punto 1 — Calculadora de servicios y semáforo de rentabilidad** (`EscandalloBuilderPage.razor` / `#escandallosHeading`): Muestra **exclusivamente los servicios activos** (`archived == false`, *"En uso"*) y **aprovecha el 100% del ancho de la pantalla** (`width: 100%; max-width: 100%; grid-column: 1 / -1;`).
    2. **Punto 2 — Catálogo de costes del centro** (`CostCatalogPage.razor` / `#catalogHeading`).
    3. **Punto 3 — Impuestos, margen objetivo y equivalencias de dosis de cabina** (`#paramsHeading`).
    4. **Punto 4 — Histórico de servicios archivados y actualizaciones de precios** (`#historyHeading`): Muestra los **servicios archivados** (`archived == true`) **ordenados por fecha de archivado descendente** (`archivedAtIso` / `archived_at DESC`), junto con el historial inmutable de cambios de precios.
    5. **Punto 5 — Diagnóstico técnico para soporte del sistema** (`#obsHeading`).
  - `MainResponsiveLayout.razor`: Layout adaptativo de ancho completo (`width: 100%; max-width: 100%;`) sin desbordamiento horizontal en móvil (`320px–767px`), tablet (`768px–1023px`) y PC (`>=1024px`) en `portrait` y `landscape`, con áreas táctiles $\ge 44\times44\text{ px}$, contraste $\ge 4.5:1$, navegación clara priorizando *"Calculadora de servicios"* en primer lugar seguida de *"Catálogo de costes"*, *"Impuestos y equivalencias"*, *"Informes de rentabilidad"* y *"Mi suscripción"*, botón accesible **"Cerrar sesión"** para revocar la sesión activa y volver de inmediato a la pantalla de inicio de sesión limpiando los datos en pantalla, y límites `CircuitOptions` ($\le 5$ circuitos WSS por usuario) (`FR-AUTH-001`, `QR-UI-A11Y-001`, `SEC-REQ-DOS-001`).
  - `EscandalloBuilderPage.razor`: Situada como **Punto 1** de la página principal ocupando el **100% del ancho disponible de la pantalla** (`.point-1-full-width` / `#workspaceMain.container` con `width: 100%; max-width: 100%;`); incluye en su cabecera las opciones globales de **descarga conjunta de todos los servicios de un golpe** (`Descargar PDF de todos los servicios` y `Descargar XLSX de todos los servicios`, en lugar de botones de descarga por servicio individual), el constructor dinámico de servicios ($1:N$) distribuido proporcionalmente a lo ancho, simulador en vivo de *Precio de venta al público (PVP)* frente al *Precio recomendado* y Semáforo de Rentabilidad con región `aria-live="polite"`. En la lista principal (`#escandallosList`) **solo aparecen los servicios activos** (`archived == false`), cada uno **colapsado por defecto en una sola línea horizontal de ancho completo (`width: 100%`)** (`<details aria-expanded="false">` con `<summary class="service-collapsed-row touch-target-44">`):
    - **En pantalla móvil (`< 768px`)**: La única línea colapsada muestra exclusivamente **nombre del servicio**, **coste total**, **precio de venta** y **semáforo de rentabilidad**.
    - **En pantallas más grandes (`>= 768px`, Tablet y PC)**: Sigue siendo de **una sola línea horizontal** repartida a lo largo de todo el ancho de la pantalla, añadiendo en la misma fila el **tiempo** (`min`), el **personal** asignado y las opciones de **duplicar** y **archivar** (`.desktop-tablet-only`).
    - Los productos cosméticos de la receta y el desglose detallado permanecen ocultos hasta que el usuario despliega la línea. Al archivar un servicio, se registra `ArchivedAtIso` (`DateTimeOffset.UtcNow.ToString("O")`), desaparece del Punto 1 y pasa a la sección de histórico (Punto 4), donde los servicios archivados se listan **ordenados por fecha de archivado descendente** (`ArchivedAtIso DESC`) con la opción de *"Restaurar servicio"* sujeta a la cuota Freemium (`FR-CALC-001`, `FR-HIST-001`, `FR-REPORT-001`, `FR-BILLING-001`, `QR-UI-A11Y-001`).
  - `CostCatalogPage.razor`: Situada como **Punto 2** para la gestión del catálogo de costes (*"Cosméticos y productos"*, *"Personal del centro"*, *"Gastos fijos del local"*, *"Importar archivo Excel"*), articulada con el **Punto 3** (*"Impuestos, margen objetivo y equivalencias de dosis de cabina"*: configuración de IVA, beneficio objetivo, jornada laboral, coeficiente de empresa, merma y calculadora de dosis de cabina de pulsaciones, gotas, cucharaditas o viales a ml/g), además de carga inicial con catálogo de ejemplo o copia de productos desde otro centro propio (`FR-COST-001`, `FR-TENANT-001`).
  - `CascadeImpactModal.razor` y `FreemiumPaywallModal.razor`: Diálogos modales accesibles (`role="dialog"`, `aria-modal="true"`) con mensajes claros orientados al usuario: aviso antes de guardar cambios de precios (*"Al actualizar el precio de este producto se recalcularán automáticamente N servicios que lo utilizan"*) y gestión de límite de servicios activos (*"Has alcanzado el límite de 2 servicios activos del plan gratuito. Archiva un servicio que no utilices ahora sin perder su historial o activa el Plan Pro por 14,90 €/mes o 149,00 €/año + IVA"*) (`FR-HIST-001`, `FR-BILLING-001`).

## 3. Protocolos de Manejo de Errores y Mitigación

- **`HTTP 400 Bad Request`**: Formatos de exportación no soportados (`csv`, `exe`), niveles de severidad inválidos (`VERBOSE99`), cuerpos de webhook $>64\text{ KB}$, firmas de webhook ausentes/inválidas/expiradas ($>300\text{ s}$) o inyección CRLF.
- **`HTTP 401 / 403 Forbidden`**: Accesos no autenticados, intentos IDOR cross-tenant en HTTP o circuitos Blazor WSS, copia de catálogo M4 desde/hacia centro ajeno o acceso de `ACT-ESTHETIC-USER-001` a endpoints de `ACT-MAINTAINER-001`.
- **`HTTP 402 Payment Required`**: Intento de activar/crear/duplicar un 3er escandallo en plan gratuito o intento de editar/exportar con $>2$ escandallos activos tras cancelación de suscripción.
- **`HTTP 409 Conflict`**: Intento de borrado físico de un coste con histórico (reconducido a baja lógica `archived`, M1).
- **`HTTP 422 Unprocessable Entity`**: Entradas aritméticas negativas, divisores cero, desbordamientos de `System.Decimal`, mermas fuera de $[0, 0.99]$, recetas con $>100$ líneas o plantillas `.xlsx` con XXE / ZipBomb ($>10\text{ MB}$ reales en `BoundedReadStream`).
- **`HTTP 429 Too Many Requests`**: Exceso de $>2$ exportaciones concurrentes, $>10$ exportaciones/min, $>30$ mutaciones con cascada/min o $>5$ circuitos Blazor WSS simultáneos (`SEC-REQ-DOS-001`).

## 4. Conformidad con la Política de Licencias (`license-policy.yaml`)

Todas las librerías del proyecto `.NET 10 LTS` (`Microsoft.AspNetCore.App`, `Blazor`, `Npgsql`, `Dapper`, `ClosedXML`, `RecyclableMemoryStream`, `PDFsharp`, `Stripe.net`, `Serilog`, `OpenTelemetry`, `xUnit`, `Reqnroll`, `bUnit`) utilizan licencias permisivas (`MIT`, `Apache-2.0`, `BSD-3-Clause`, `PostgreSQL`) clasificadas como `ALLOW` en `license-policy.yaml`.

## 5. Historial de Revisiones

| Versión | Fecha | Autor | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-06 | agent-qa-engineer | Diseño técnico formal en .NET Puro alineado con ADR-001 a ADR-005 | CHG-001-ESCANDALLO-MVP |
| **1.1.0** | 2026-10-08 | agent-qa-engineer | Sincronización con arquitectura y seguridad v1.1.0 (.NET 10 LTS + Blazor + PostgreSQL 16 FORCE RLS) | CHG-001-ESCANDALLO-MVP |
| **1.2.0** | 2026-10-09 | agent-system-architect | Reordenación de secciones en página principal (1. Calculadora de servicios, 2. Catálogo de costes, 3. Impuestos y equivalencias) y servicios colapsados por defecto con resumen y productos ocultos | CHG-001-ESCANDALLO-MVP |
| **1.3.0** | 2026-10-09 | agent-system-architect | Filtrado exclusivo de servicios activos en la calculadora principal (Punto 1) y listado de servicios archivados en la sección de histórico (Punto 4) ordenados por fecha de archivado descendente (`ArchivedAtIso DESC`) | CHG-001-ESCANDALLO-MVP |
| **1.4.0** | 2026-10-09 | agent-system-architect | Componente de servicio colapsado en una sola línea (móvil: nombre, coste total, precio de venta y semáforo; pantallas mayores añade tiempo, personal, duplicar y archivar) y descarga conjunta PDF/XLSX de todos los servicios de un golpe | CHG-001-ESCANDALLO-MVP |
| **1.5.0** | 2026-10-09 | agent-system-architect | Aprovechamiento del 100% del ancho de la pantalla en el Punto 1 (`CMP-WEB-UI-001` v1.6.0) y maquetación ejecutiva del PDF para Dirección con Resumen Ejecutivo inicial (vista colapsada) seguido del listado de cada servicio desglosado (`CMP-DOC-IO-001` v1.2.0) | CHG-001-ESCANDALLO-MVP |
