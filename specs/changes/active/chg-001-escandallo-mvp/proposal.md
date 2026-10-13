---
id: CHG-001-ESCANDALLO-MVP
type: spec-change-proposal
title: MVP Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética
status: proposed
author: agent-qa-engineer
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

# Propuesta de Cambio: CHG-001-ESCANDALLO-MVP

## 1. Motivación y Alcance

Materialización del incremento inicial (**MVP**) de la plataforma SaaS Multi-Tenant de cálculo de escandallos, beneficios y fijación de PVP para centros de estética en **.NET 10 LTS (`C# 14`, `ASP.NET Core Minimal APIs` + `Blazor Web App` + `PostgreSQL 16` con `FORCE ROW LEVEL SECURITY`)**, cubriendo:
1. **Aislamiento Multi-Tenant Zero-Trust y Copia de Catálogo (`FR-TENANT-001`, `SEC-REQ-TENANT-001`, M4)**: Gestión multi-centro por propietario con `PostgreSQL FORCE ROW LEVEL SECURITY`, rol `NOBYPASSRLS`, fijación transaccional `SET LOCAL` (`set_config(..., true)`) en el pool `Npgsql`, revalidación en circuitos Blazor SignalR y validación dual transaccional en copia de catálogo entre centros propios.
2. **Catálogo de Costes, Plantilla Semilla/Excel y Merma (`FR-COST-001`, `SEC-REQ-XLSX-001`, M1, M4, M5)**: Cálculo exacto en `System.Decimal` (128-bit) con guardas anti-`OverflowException` de Productos (`ml`, `g`, `ud` con `% Merma`), Personal (`((Neto * Retenciones) + Neto) * Coef_Empresa / Minutos_Mes`), Gastos Generales por minuto y Parámetros (`IVA`, `Beneficio`), baja lógica (`archived`, M1), lectura acotada con `BoundedReadStream` ($\le 10\text{ MB}$ reales descomprimidos sobre `RecyclableMemoryStream`) y neutralización DDE con prefijo `'` y `XLDataType.Text` en `ClosedXML`.
3. **Motor de Escandallo, Simulación de PVP y Semáforo de Margen (`FR-CALC-001`, `FR-HIST-001`, `SEC-REQ-DOS-001`, M2, M3)**: Constructor dinámico $1:N$ ($\le 100$ líneas), previsualización de impacto antes de guardar (*"Este cambio recalculará N escandallos"*), recálculo transaccional en cascada con histórico condicional inmutable y evaluación del Semáforo de Erosión de Margen (`OPTIMO` 🟢 / `ALERTA` 🟡 / `CRITICO` 🔴).
4. **Informes Ejecutivos Exclusivos en PDF y Excel (`FR-REPORT-001`, `SEC-REQ-DOS-001`)**: Exportación acotada por `SemaphoreSlim(2, 2)` ($\le 2$ simultáneas y $\le 10/\text{min}$ por centro) sobre `RecyclableMemoryStream` en `.pdf` (`PDFsharp`) y `.xlsx` (`ClosedXML`), más cota de $\le 5$ circuitos `InteractiveServer` WSS por usuario.
5. **Cuota Freemium Atómica y Suscripción Stripe (`FR-BILLING-001`, `SEC-REQ-BILLING-001`, M1)**: Límite gratuito de 2 escandallos `active` por centro protegido con `SELECT ... FOR UPDATE`, tarifas de **14,90 EUR/mes** y **149,00 EUR/año** (+ IVA), verificación `Stripe.EventUtility.ConstructEvent` sobre cuerpo crudo ($\le 64\text{ KB}$, HMAC-SHA256 $\le 300\text{ s}$), idempotencia monótona y desbloqueo post-cancelación mediante archivado sin pérdida de datos.
6. **Observabilidad Dual y Nivel en Caliente (`FR-OBS-001`, `SEC-REQ-OBS-001`)**: Extracción en `JSON`, `CSV` y `OTLP` para `ACT-MAINTAINER-001`, cambio de severidad en caliente (`LoggingLevelSwitch` con TTL de $60\text{ min}$), cota `ToMaximumDepth(3)` y redactado inmutable `[REDACTED]` (incluyendo `Exception.Message`) junto con canal protegido para eventos `SECURITY`.

## 2. Dependencias Externas Evaluadas (`license-policy.yaml`)

| Paquete NuGet (.NET 10 LTS) | Propósito | Licencia SPDX | Veredicto `license-policy.yaml` |
| :--- | :--- | :---: | :---: |
| `Microsoft.AspNetCore.App` / `Blazor` | Servidor web Kestrel, Minimal APIs y UI accesible WCAG 2.1 AA | `MIT` | `ALLOW` |
| `Npgsql` + `Dapper` / `EF Core 10` | Acceso a datos PostgreSQL 16 con soporte RLS transaccional | `PostgreSQL` / `Apache-2.0` / `MIT` | `ALLOW` |
| `ClosedXML` + `RecyclableMemoryStream` | Importación y exportación de hojas `.xlsx` sin fragmentación de LOH | `MIT` | `ALLOW` |
| `PDFsharp` (`PdfSharpCore`) | Generación de informes ejecutivos `.pdf` | `MIT` | `ALLOW` |
| `Stripe.net` | Integración oficial de Checkout, Portal y Webhooks HMAC | `MIT` | `ALLOW` |
| `Serilog` + `OpenTelemetry` | Logs estructurados JSON, trazas W3C y métricas OTLP | `Apache-2.0` | `ALLOW` |
| `xUnit` + `Reqnroll` + `bUnit` | Ejecución de suites BDD Gherkin y pruebas unitarias en rojo/verde | `Apache-2.0` / `BSD-3-Clause` / `MIT` | `ALLOW` |
