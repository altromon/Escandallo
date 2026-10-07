---
id: CHG-001-ESCANDALLO-MVP
type: spec-change-proposal
title: MVP Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética
status: applied
author: agent-qa-engineer
citations:
  - id: FR-TENANT-001
    digest: sha256:5b5c4c6755e161bf1540458ca454893f98fc274e66a1b5e8e740a96159adda96
    comment: Citación de requirement (Aislamiento Multi-Tenant de Centros de Estética por Cliente)
  - id: FR-COST-001
    digest: sha256:26c54ebe40304843c5d455bb6c2640a83224b8a484f19dee6d0e4f6ff5e85422
    comment: Citación de requirement (Alta y Cálculo Unitario de Productos, Personal, Gastos Generales y Parámetros)
  - id: FR-CALC-001
    digest: sha256:3acd854bec77aa4474be77352eb913c72b538a27c332ab5ac73ff75b3a2d19d4
    comment: Citación de requirement (Cálculo de Escandallo Total, Beneficio y PVP de Servicios Estéticos)
  - id: FR-HIST-001
    digest: sha256:7085dcad6c88b320df3e4bc295493bc4e4ef148872287d5dbd79b335ff269803
    comment: Citación de requirement (Registro Histórico y Recálculo Automático en Cascada de Escandallos)
  - id: FR-REPORT-001
    digest: sha256:e5417999bf60a8a8459942976dbcbccc13d1e6fa6f4eb2252806bbced2c78b23
    comment: Citación de requirement (Generación y Exportación de Informes Ejecutivos de Costes y Beneficios)
  - id: FR-BILLING-001
    digest: sha256:91a229dd4c2480196cdeb4c16fad4228ea2a9e23585a7e6262d84f8d821c7930
    comment: Citación de requirement (Control de Cuota Gratuita de 2 Escandallos y Suscripción Ilimitada por Centro con Stripe)
  - id: FR-OBS-001
    digest: sha256:c5e7bcc62b4bbf9e2efb4ee34e1eea185d0254879562a66e378e619d7f346cd0
    comment: Citación de requirement (Extracción de Logs, Trazas y Métricas del Servidor para Usuario Mantenedor)
  - id: QR-UI-A11Y-001
    digest: sha256:f160125f01cd757442ecbda09eff36b7d2fc07fa62e9d24ef7b9a37186816373
    comment: Citación de requirement (Interfaz Web Accesible (WCAG 2.1 AA) y Adaptativa para PC, Tablet y Móvil)
  - id: QR-SEC-001
    digest: sha256:b94b7be77d41f765cd4c0aa74fdb98befac56577e2e1f47241ef58e2f6ba94bd
    comment: Citación de requirement (Seguridad de Servidor frente a Ciberataques, Protección IDOR y Verificación de Webhooks)
  - id: SEC-REQ-TENANT-001
    digest: sha256:024fa02b39646a6d57902fb76289506f1343319732cb6d68effd33401e520155
    comment: Citación de security-requirement (Autorización Multi-Tenant Zero-Trust, Prevención IDOR/BOLA y Row-Level Security)
  - id: SEC-REQ-BILLING-001
    digest: sha256:4e5273a8b0bca455db2e0f7c5a88f7280c4d517e32fea826ad1299193537e22f
    comment: Citación de security-requirement (Control Atómico Anti-Race-Condition de Cuota Freemium y Verificación HMAC de Webhooks Stripe)
  - id: SEC-REQ-XLSX-001
    digest: sha256:c4c729dfb22030176e9f10a92e050997d493013e46560e41a0770bdd08cff30b
    comment: Citación de security-requirement (Parseo Seguro de Plantillas Excel Anti-XXE/ZipBomb y Neutralización de Fórmulas DDE)
  - id: SEC-REQ-DOS-001
    digest: sha256:24f4765cf5dee7af61434b44b35f1166958bcc3165af7c7d443a9d4dbb584260
    comment: Citación de security-requirement (Rate Limiting por Inquilino, Cuotas de Concurrencia en Exportación PDF/Excel y Límites de Recálculo)
  - id: SEC-REQ-OBS-001
    digest: sha256:fc0f4c5cb153c0ab6e2d5a02ef7038a30e9a3e109d0cafa84d046e261bd36cc6
    comment: Citación de security-requirement (RBAC Estricto para Mantenedor, Redactado Automático PII/Secretos en Logs DEBUG y Anti-Log-Injection)
  - id: UC-COST-CATALOG-001
    digest: sha256:7360656256042b1c91bce0dc5f109fd57780817488c3ecf6deb69713f96eb0cd
    comment: Citación de use-case (Alta, Edición e Histórico del Catálogo de Costes y Parámetros por Centro de Estética)
  - id: UC-ESCANDALLO-CALC-001
    digest: sha256:f6fdf9ad53c9b0bf6af517af04cd5bf28ee22dab9bb4c75cbfb5f0ac9f00734f
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
    digest: sha256:11ef288997add9796a8bbf8974fea87ac975586b1bbfe80322a89de9a542c989
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
    digest: sha256:e236d0516dd57d8e1c2f4cfccbcbba83a6934180a592ceb7631a9c4ad204ccd4
    comment: Citación de component (Plataforma SaaS Multi-Tenant de Escandallos y Rentabilidad para Centros de Estética (Sistema Raíz))
  - id: CMP-WEB-UI-001
    digest: sha256:8a34f7caa2d7be50c2f752f115e31615790022eb324029102170b79c3c205f55
    comment: Citación de component (Cliente Web Adaptativo y Accesible (SPA/SSR Responsive UI para PC, Tablet y Móvil))
  - id: CMP-IAM-TENANT-001
    digest: sha256:3c0a795801b8bb269418726c7f5e5bd3881529d8bfa79cc56a4a32f0d26a70ec
    comment: Citación de component (Módulo de Identidad, RBAC, Guardia Multi-Tenant RLS y Limitador de Tasa)
  - id: CMP-COST-ENGINE-001
    digest: sha256:7430363752fa6bfe3b44fd517341bdb83f4500a0c0e2b397743f12c9d6fbb783
    comment: Citación de component (Motor de Catálogo de Costes, Fórmulas de Escandallo, Semáforo de Margen y Recálculo en Cascada)
  - id: CMP-DOC-IO-001
    digest: sha256:f413cc3ded7634ff66ca5f704d890240044e21beeb1913b8c340674bcac78dca
    comment: Citación de component (Subsistema de Importación Segura de Catálogos (.xlsx) y Exportación de Informes Ejecutivos (PDF / Excel))
  - id: CMP-BILLING-STRIPE-001
    digest: sha256:59466bb93c0d5875b6ea4c6deca782e273842cebb91f094b6dbdf31f7b7a8d50
    comment: Citación de component (Módulo de Suscripciones, Control Atómico de Cuota Freemium e Integración con Stripe)
  - id: CMP-OBS-TELEMETRY-001
    digest: sha256:1d830f3d88a9429d53747db37a5e227fa7874f204db6e17e0e0b80c798c139f1
    comment: Citación de component (Subsistema de Observabilidad Dual, Nivel de Log en Caliente, Redactado de Secretos y Consola de Mantenedor)
  - id: ADR-001-COST-EFFECTIVE-DEPLOYMENT
    digest: sha256:1039232a88fc1f2cbf10610d7d6da48c00b55963343da2a045c6864815e0950c
    comment: 'Citación de architecture-decision-record (Topología de Despliegue Ultra-Rentable y Segura: Cloudflare Edge WAF + Monolito Modular Dockerizado en VPS Europeo)'
  - id: ADR-002-MODULAR-MONOLITH-STACK
    digest: sha256:1fe566ac520cd633d6bc3e9ca52b824a7df8a6e0cbb4d443a13b31442d4e0480
    comment: Citación de architecture-decision-record (Arquitectura de Monolito Modular Full-Stack en TypeScript (Node.js LTS + Fastify/React + PostgreSQL) Alineada con license-policy.yaml)
  - id: ADR-003-MULTITENANT-RLS-AND-ATOMIC-QUOTA
    digest: sha256:4c705accc12130aa1d2d120e7bece3b0823c31edd25fdee2da1966bba6b2eaf0
    comment: Citación de architecture-decision-record (Aislamiento Multi-Tenant con PostgreSQL Row-Level Security (RLS) y Bloqueo Pesimista para Cuota Freemium de Stripe)
  - id: ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE
    digest: sha256:2d368010ffa0bb844958765e289a522ad6aae67a53a177295de8a41283d9d355
    comment: Citación de architecture-decision-record (Aritmética Decimal Exacta (decimal.js / NUMERIC) y Recálculo Transaccional Síncrono en Cascada con Snapshots Inmutables)
  - id: ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS
    digest: sha256:b69fb62f4ee2e604a612738f51d94609abcaa2c7c9a9b9460f87ca3d519bb54c
    comment: Citación de architecture-decision-record (Observabilidad Dual Ligera (Pino + OpenTelemetry) con Nivel en Caliente y Motor Documental Seguro en Streaming (ExcelJS / PDFKit))
---


# Propuesta de Cambio: CHG-001-ESCANDALLO-MVP

## 1. Motivación y Alcance

Materialización del incremento inicial (**MVP**) de la plataforma SaaS Multi-Tenant de cálculo de escandallos, beneficios y fijación de PVP para centros de estética en **.NET Puro (`C# 12/13`, `.NET 8/9 LTS`, `ASP.NET Core Minimal APIs` + `Blazor Web App` + `PostgreSQL 16`)**, cubriendo:
1. **Aislamiento Multi-Tenant y Copia de Catálogo (`FR-TENANT-001`, `SEC-REQ-TENANT-001`, M4)**: Gestión multi-centro por propietario con `PostgreSQL FORCE ROW LEVEL SECURITY` y validación dual transaccional en copia de catálogo entre centros propios.
2. **Catálogo de Costes, Plantilla Semilla/Excel y Merma (`FR-COST-001`, `SEC-REQ-XLSX-001`, M1, M4, M5)**: Cálculo exacto en `System.Decimal` de Productos (`ml`, `g`, `ud` con `% Merma`), Personal (`((Neto * Retenciones) + Neto) * Coef_Empresa / Minutos_Mes`), Gastos Generales por minuto y Parámetros (`IVA`, `Beneficio`), con baja lógica (`archived`, M1) y pre-inspección anti-XXE/ZipBomb/DDE.
3. **Motor de Escandallo, Simulación de PVP y Semáforo de Margen (`FR-CALC-001`, `FR-HIST-001`, `SEC-REQ-DOS-001`, M2, M3)**: Constructor dinámico $1:N$ ($\le 100$ líneas), previsualización de impacto antes de guardar (*"Este cambio recalculará N escandallos"*), recálculo transaccional en cascada con histórico condicional inmutable y evaluación del Semáforo de Erosión de Margen (`OPTIMO` 🟢 / `ALERTA` 🟡 / `CRITICO` 🔴).
4. **Informes Ejecutivos Exclusivos en PDF y Excel (`FR-REPORT-001`, `SEC-REQ-DOS-001`)**: Exportación acotada por `SemaphoreSlim` ($\le 2$ simultáneas por centro) en `.pdf` (`PDFsharp`) y `.xlsx` (`ClosedXML`).
5. **Cuota Freemium Atómica y Suscripción Stripe (`FR-BILLING-001`, `SEC-REQ-BILLING-001`, M1)**: Límite gratuito de 2 escandallos `active` por centro protegido con `SELECT ... FOR UPDATE`, tarifas de **14,90 EUR/mes** y **149,00 EUR/año** (+ IVA), verificación HMAC-SHA256 ($\le 300\text{ s}$) y desbloqueo post-cancelación mediante archivado sin pérdida de datos.
6. **Observabilidad Dual y Nivel en Caliente (`FR-OBS-001`, `SEC-REQ-OBS-001`)**: Extracción en `JSON`, `CSV` y `OTLP` para `ACT-MAINTAINER-001`, cambio de severidad en caliente (`LoggingLevelSwitch`) y redactado inmutable `[REDACTED]` incluso en modo `DEBUG`.

## 2. Dependencias Externas Evaluadas (`license-policy.yaml`)

| Paquete NuGet (.NET 8/9) | Propósito | Licencia SPDX | Veredicto `license-policy.yaml` |
| :--- | :--- | :---: | :---: |
| `Microsoft.AspNetCore.App` / `Blazor` | Servidor web Kestrel, Minimal APIs y UI accesible WCAG 2.1 AA | `MIT` | `ALLOW` |
| `Npgsql` + `Dapper` | Acceso a datos PostgreSQL 16 con soporte RLS transaccional | `PostgreSQL` / `Apache-2.0` | `ALLOW` |
| `ClosedXML` | Importación y exportación de hojas `.xlsx` en streaming | `MIT` | `ALLOW` |
| `PDFsharp` (`PdfSharpCore`) | Generación de informes ejecutivos `.pdf` | `MIT` | `ALLOW` |
| `Stripe.net` | Integración oficial de Checkout, Portal y Webhooks HMAC | `MIT` | `ALLOW` |
| `Serilog` + `OpenTelemetry` | Logs estructurados JSON, trazas W3C y métricas OTLP | `Apache-2.0` | `ALLOW` |
| `xUnit` + `Reqnroll` + `bUnit` | Ejecución de suites BDD Gherkin y pruebas unitarias en rojo/verde | `Apache-2.0` / `BSD-3-Clause` / `MIT` | `ALLOW` |
