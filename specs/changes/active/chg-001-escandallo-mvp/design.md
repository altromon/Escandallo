---
id: DSG-CHG-001-ESCANDALLO-MVP
type: spec-design
change-id: CHG-001-ESCANDALLO-MVP
title: "Diseño Técnico: MVP Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética"
version: "1.0.0"
schema-version: "1.0"
handoff: "HOF-CHG-001-ESCANDALLO-MVP"
architecture-component: "CMP-ESCANDALLO-SYS-001"
enclave: "SEC-ENC-APP-CORE-001"
status: draft
citations:
  - id: FR-TENANT-001
    digest: sha256:4af3ae7c7abb0df219f8ad60cca1f313ee5c4e4259bde9c0a9e1a8fe42576902
    comment: Citación de requirement (Aislamiento Multi-Tenant de Centros de Estética por Cliente)
  - id: FR-COST-001
    digest: sha256:29ccbec3cfbfde6ec90b588574aaa9c74193dae445971629dfb7cb4ce38944fb
    comment: Citación de requirement (Alta y Cálculo Unitario de Productos, Personal, Gastos Generales y Parámetros)
  - id: FR-CALC-001
    digest: sha256:66113f2d558963c78d2fd0c37f31b9e06e1311593cc58451e0fd8d8cbe767a49
    comment: Citación de requirement (Cálculo de Escandallo Total, Beneficio y PVP de Servicios Estéticos)
  - id: FR-HIST-001
    digest: sha256:51b01d6172a0c1d0291625e74e6c449d16173045cc6e1ad1d64514113c41ce50
    comment: Citación de requirement (Registro Histórico y Recálculo Automático en Cascada de Escandallos)
  - id: FR-REPORT-001
    digest: sha256:c98f064a110000724f22d9b8842cba851a4f86e1c8db5267ef616608ef927641
    comment: Citación de requirement (Generación y Exportación de Informes Ejecutivos de Costes y Beneficios)
  - id: FR-BILLING-001
    digest: sha256:52460917ab0c9de73ceeec6cede8b903980c71d58d08d61c083c8b71f0dba770
    comment: Citación de requirement (Control de Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro con Stripe)
  - id: FR-OBS-001
    digest: sha256:60dfcffa453c38b4da1b9ef210396779367996bc0d41e4d83d34dbe631e6c087
    comment: Citación de requirement (Extracción de Logs, Trazas y Métricas del Servidor para Usuario Mantenedor)
  - id: QR-UI-A11Y-001
    digest: sha256:920867ccce542eb0419fc0d1687d592c2e045c09d2ff6b662d5753a38040b2e6
    comment: Citación de requirement (Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil)
  - id: QR-SEC-001
    digest: sha256:516ea0486ee0625aa88a4634d90505b9141e056552d4034bc463af72bc75ef53
    comment: Citación de requirement (Seguridad de Servidor frente a Ciberataques, Protección IDOR y Verificación de Webhooks)
  - id: SEC-REQ-TENANT-001
    digest: sha256:5e007ab61b86b67fea5a7c1a01bfcbaf3b04c3571df9b745c817c5f6760844a4
    comment: Citación de security-requirement (Autorización Multi-Tenant Zero-Trust, Prevención IDOR/BOLA y Row-Level Security)
  - id: SEC-REQ-BILLING-001
    digest: sha256:01208a9f23bdaccbb555e32b289132ae006f5c22dfbe44a7014f38980adf2ea4
    comment: Citación de security-requirement (Control Atómico Anti-Race-Condition de Cuota Freemium y Verificación HMAC de Webhooks Stripe)
  - id: SEC-REQ-XLSX-001
    digest: sha256:52fea021930bb89ccbe31f7d9de2b722b96b9e0892e317bdc79780430bcb26ec
    comment: Citación de security-requirement (Parseo Seguro de Plantillas Excel Anti-XXE/ZipBomb y Neutralización de Fórmulas DDE)
  - id: SEC-REQ-DOS-001
    digest: sha256:332e9f8b7c2207f1a0fc6ab9cc622c68384328ffd36378442a9a3e899c8eb94f
    comment: Citación de security-requirement (Rate Limiting por Inquilino, Cuotas de Concurrencia en Exportación PDF/Excel y Límites de Recálculo)
  - id: SEC-REQ-OBS-001
    digest: sha256:daa1c3a0c5528c985eae07caecdc2f5e4c443990630c55392c21efc95ef47e04
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
    digest: sha256:c609cac145bd6aa1acad487b4279d0b6b1acbaff593aee9e4de2ddb669046946
    comment: Citación de abuse-case (Exfiltración Cross-Tenant de Costes, Catálogos e Informes mediante IDOR/BOLA)
  - id: ABUSE-QUOTA-BYPASS-001
    digest: sha256:1988af7080ef86b2920eaf5a855e7accaf2be462923be38649a3e50d683c71e5
    comment: Citación de abuse-case (Evasión de Cuota Freemium mediante Condiciones de Carrera y Spoofing de Webhooks Stripe)
  - id: ABUSE-XLSX-INJECT-001
    digest: sha256:3b629cc3c1b333f215d1c0485eea3d8f204458913477095c25b7b2c089990a09
    comment: Citación de abuse-case (Inyección de Fórmulas DDE, XXE y Bombas ZIP en Importación y Exportación de Excel)
  - id: ABUSE-DOS-CASCADE-001
    digest: sha256:4d9e8215150c37458c2150565a69f72ba894dba71a154be8b659090deffb3ae7
    comment: Citación de abuse-case (Denegación de Servicio Asimétrica por Tormenta de Recálculos en Cascada y Renderizado PDF)
  - id: ABUSE-PRIV-OBS-001
    digest: sha256:e0d169604c5e3b28e609300b86fd9a055334b1e197200372c0e6f1b38633690a
    comment: Citación de abuse-case (Escalada de Privilegios al Rol Mantenedor, Log Forgery y Fuga de Secretos en Modo DEBUG)
  - id: CMP-ESCANDALLO-SYS-001
    digest: sha256:f5227af29aec2f523533459c87b81dee014335c083c328459438f2e3fd9d73c2
    comment: Citación de component (Plataforma SaaS Multi-Tenant de Escandallos y Rentabilidad para Centros de Estética (Sistema Raíz))
  - id: CMP-WEB-UI-001
    digest: sha256:6717e96126b7e170148a4e59742336fb8814537e09911bfe69a1f47470473012
    comment: Citación de component (Cliente Web Adaptativo y Accesible (SPA/SSR Responsive UI para PC, Tablet y Móvil))
  - id: CMP-IAM-TENANT-001
    digest: sha256:26e35d9009c41c3895617dffec65f6aa75a2316297d4bfd57bc5210786526b4c
    comment: Citación de component (Módulo de Identidad, RBAC, Guardia Multi-Tenant RLS y Limitador de Tasa)
  - id: CMP-COST-ENGINE-001
    digest: sha256:6bfd4b56fb538eb1533743df940fc9a621e92e65efbc2ff5c2f60e36f5415f6a
    comment: Citación de component (Motor de Catálogo de Costes, Fórmulas de Escandallo, Semáforo de Margen y Recálculo en Cascada)
  - id: CMP-DOC-IO-001
    digest: sha256:209c9c449fca0ac83af824ce3199eb68942882e975e3fe95f1ade54b4e97f31e
    comment: Citación de component (Subsistema de Importación Segura de Catálogos (.xlsx) y Exportación de Informes Ejecutivos (PDF / Excel))
  - id: CMP-BILLING-STRIPE-001
    digest: sha256:5a2fd2ae7770ab5f31017be2ae523e491bd45f3a0cbd08e5ab700068312654ac
    comment: Citación de component (Módulo de Suscripciones, Control Atómico de Cuota Freemium e Integración con Stripe)
  - id: CMP-OBS-TELEMETRY-001
    digest: sha256:d305943158360840e753e8e1f326ad86868f3d94d8f18ef6b1147565515b968a
    comment: Citación de component (Subsistema de Observabilidad Dual, Nivel de Log en Caliente, Redactado de Secretos y Consola de Mantenedor)
  - id: ADR-001-COST-EFFECTIVE-DEPLOYMENT
    digest: sha256:ddb801eacbba3ae06b68086f6b1bb00c43885fcbb715960d7e1e4de3c35ca8ee
    comment: 'Citación de architecture-decision-record (Topología de Despliegue Ultra-Rentable y Segura: Cloudflare Edge WAF + Monolito Modular Dockerizado en VPS Europeo)'
  - id: ADR-002-MODULAR-MONOLITH-STACK
    digest: sha256:8124bc96c46d93f34b2da553039a72263d303b9549feff18f1bd3c79b09301e6
    comment: Citación de architecture-decision-record (Arquitectura de Monolito Modular Full-Stack en TypeScript (Node.js LTS + Fastify/React + PostgreSQL) Alineada con license-policy.yaml)
  - id: ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA
    digest: sha256:e515c71fe55a4f03dcb65d44c2c54383d336879580c1c994e15f9aaf0f12171d
    comment: Citación de architecture-decision-record (Aislamiento Multi-Tenant con PostgreSQL Row-Level Security (RLS) y Bloqueo Pesimista para Cuota Freemium de Stripe)
  - id: ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE
    digest: sha256:e15a718668d17c8ae6d80221f3439b79a6c2422dfe7b9550efb76a4381be6e8c
    comment: Citación de architecture-decision-record (Aritmética Decimal Exacta (decimal.js / NUMERIC) y Recálculo Transaccional Síncrono en Cascada con Snapshots Inmutables)
  - id: ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS
    digest: sha256:870bf57801a9a66e066bd5b0dd219405a4b4ea65d1966c3d26dcb2c75f8854d5
    comment: Citación de architecture-decision-record (Observabilidad Dual Ligera (Pino + OpenTelemetry) con Nivel en Caliente y Motor Documental Seguro en Streaming (ExcelJS / PDFKit))
---

# Diseño Técnico: CHG-001-ESCANDALLO-MVP

## 1. Mapeo Arquitectónico y Enclave Zero Trust
El incremento `CHG-001-ESCANDALLO-MVP` materializa el sistema raíz `CMP-ESCANDALLO-SYS-001` y sus 6 subsistemas de Nivel 2 (`CMP-WEB-UI-001`, `CMP-IAM-TENANT-001`, `CMP-COST-ENGINE-001`, `CMP-DOC-IO-001`, `CMP-BILLING-STRIPE-001`, `CMP-OBS-TELEMETRY-001`) distribuidos en los 3 enclaves (`SEC-ENC-EDGE-WAF-001`, `SEC-ENC-APP-CORE-001`, `SEC-ENC-DATA-OBS-001`).

## 2. Contratos de Datos e Interfaces en C# (`src/Escandallo.Core/Modules/`)
- `src/Escandallo.Core/Modules/Iam/IamTenantGuard.cs`: `AuthorizeCrossCenterCopy`, `VerifySessionAndRls`, `CheckTenantRateLimit`.
- `src/Escandallo.Core/Modules/CostEngine/CostCatalogCalculator.cs`: `CalculateOverheadRatePerMinute`, `CalculateProductUnitCost`, `CalculateStaffUnitCost` (usando `System.Decimal` de 128 bits).
- `src/Escandallo.Core/Modules/CostEngine/EscandalloCalculator.cs`: `CalculateEscandalloService`, `EvaluateMarginSemaphore`.
- `src/Escandallo.Core/Modules/CostEngine/CascadeHistoryService.cs`: `ApplyCostMutationWithCascade`, `ArchiveCostOrService`.
- `src/Escandallo.Core/Modules/DocIo/DocumentIoService.cs`: `ExportExecutiveReport`, `InspectAndParseExcelCatalog`, `SanitizeSpreadsheetCell`.
- `src/Escandallo.Core/Modules/Billing/BillingQuotaService.cs`: `EnforceFreemiumQuotaWithLock`, `VerifyAndProcessStripeWebhook`.
- `src/Escandallo.Core/Modules/Observability/TelemetryService.cs`: `SetHotLogLevel`, `ExportTelemetryBundle`, `RedactTelemetryRecord`.
- `src/Escandallo.Core/Modules/WebUi/AccessibilityValidator.cs`: `EvaluateDynamicRecipeBuilderA11y`, `ValidateResponsiveViewportLayout`.

## 3. Protocolos de Manejo de Errores y Mitigación
- Errores de dominio aritmético, divisores cero, mermas $> 100\%$ o archivos `.xlsx` maliciosos devuelven `HTTP 422 Unprocessable Entity` con código tipado.
- Accesos cross-tenant devuelven `HTTP 403 Forbidden` / `404 Not Found` y emiten evento `SECURITY`.
- Excesos de cuota gratuita devuelven `HTTP 402 Payment Required`.
- Excesos de concurrencia o frecuencia devuelven `HTTP 429 Too Many Requests` con cabecera `Retry-After`.

## 4. Conformidad con la Política de Licencias (`license-policy.yaml`)
Todas las librerías empleadas cumplen estrictamente con las categorías permisivas (`MIT`, `Apache-2.0`) de `license-policy.yaml`, sin ninguna dependencia copyleft (`GPL`/`AGPL`/`SSPL`).

## 5. Historial de Revisiones

| Versión | Fecha | Autor | Descripción del Cambio | Referencia |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-qa-engineer | Diseño técnico formal inicial y contratos de módulos para TDD/BDD | CHG-001-ESCANDALLO-MVP |

