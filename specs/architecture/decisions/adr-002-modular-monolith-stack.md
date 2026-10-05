---
id: ADR-002-MODULAR-MONOLITH-STACK
type: architecture-decision-record
title: "Arquitectura de Monolito Modular en C# (.NET + ASP.NET Core / Blazor + PostgreSQL) Alineada con license-policy.yaml"
status: proposed
version: "1.1.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "Product Owner"
decision-date: "2026-10-05"
affects-components:
  - CMP-ESCANDALLO-SYS-001
  - CMP-WEB-UI-001
  - CMP-IAM-TENANT-001
  - CMP-COST-ENGINE-001
  - CMP-DOC-IO-001
  - CMP-BILLING-STRIPE-001
  - CMP-OBS-TELEMETRY-001
supersedes: null
superseded-by: null
---

# ADR-002: Arquitectura de Monolito Modular en C# (.NET + ASP.NET Core / Blazor + PostgreSQL) Alineada con license-policy.yaml

## 1. Context and Problem Statement

El sistema requiere una aplicación web accesible (WCAG 2.1 AA) y optimizada para PC, tablet y móvil (`QR-UI-A11Y-001`), un motor de cálculo financiero determinista (`FR-CALC-001`) alineado con el proyecto de referencia en C# (`EscandalloVibeCoded`), generación de informes ejecutivos en PDF y Excel (`FR-REPORT-001`), integración con Stripe (`FR-BILLING-001`) y observabilidad avanzada (`FR-OBS-001`). Además, todo el código fuente debe cumplir las métricas estrictas de `quality-policy.yaml` (complejidad ciclomática $\le 10$, complejidad cognitiva $\le 15$, índice de mantenibilidad $\ge 50$, longitud de función $\le 40$ líneas) y el árbol de dependencias debe utilizar exclusivamente licencias permisivas aprobadas en `license-policy.yaml` (`MIT`, `Apache-2.0`, `BSD`, `ISC`).

## 2. Considered Technology Options

1. **Opción A (Elegida): Monolito Modular Hexagonal en C# (`.NET` + `ASP.NET Core` + `Blazor Web App` + `Npgsql/Dapper/EF Core` + `PostgreSQL 16`)**:
   - **Pros**: Alineación total con el dominio original de `EscandalloVibeCoded`, tipo primitivo financiero **`decimal` de 128 bits en base 10** (`System.Decimal`) nativo en el CLR, huella de memoria reducida en contenedores Linux (`~70–90 MB RAM`), altísimo throughput en CPU y soporte nativo en el analizador políglota de `@aisdlc/core` (`.cs`).
   - **Licencias**: 100% `MIT` / `Apache-2.0` / `BSD`.
2. **Opción B: Arquitectura de Microservicios Distribuida (API Gateway + Servicio Costes + Servicio Escandallos + Servicio Reportes + Broker Kafka/RabbitMQ)**:
   - **Contras**: Multiplica por $5\times$ el consumo de RAM y CPU, introduce latencia de serialización de red y transacciones distribuidas (Sagas) innecesarias para un dominio cuyas operaciones de recálculo son fuertemente consistentes (`BR-RECALC-HISTORY-001`). Viola el principio de simplicidad (YAGNI).
3. **Opción C: Stack Java/Spring Boot o Python/Django con Celery/Redis**:
   - **Contras**: Mayor huella de memoria base en contenedor (JVM requiere $\ge 1\text{ GB RAM}$ por servicio; Celery + Redis añade procesos adicionales), encareciendo el VPS mínimo necesario.

## 3. Decision Outcome

Se adopta la **Opción A (Monolito Modular Hexagonal en C# sobre .NET y PostgreSQL 16)**, estructurado internamente en módulos de dominio desacoplados bajo `Escandallo.Core.Modules` (`Iam`, `CostEngine`, `DocIo`, `Billing`, `Observability`, `WebUi`) y verificado con `xUnit` en `Escandallo.UnitTests`.

### Catálogo de Librerías Aprobadas (`license-policy.yaml` Compliance)

| Capacidad | Paquete NuGet Seleccionado | Licencia SPDX | Estado en `license-policy.yaml` | Alternativa Descartada (Motivo) |
| :--- | :--- | :---: | :---: | :--- |
| Servidor HTTP / API & Web UI | `Microsoft.AspNetCore.App` (ASP.NET Core + Blazor) | `MIT` | `ALLOW` | — |
| Aritmética Financiera | `System.Decimal` (`decimal` 128-bit nativo CLR) | `MIT` | `ALLOW` | `double`/`float` IEEE 754 (deriva de redondeo) |
| Acceso a Datos SQL | `Npgsql` + `Dapper` / `EF Core` | `PostgreSQL` / `Apache-2.0` / `MIT` | `ALLOW` | — |
| Generación PDF | `PDFsharp` (`PdfSharpCore`) | `MIT` | `ALLOW` | `iText` (`AGPL-3.0`) y `QuestPDF` (Licencia comercial requerida superado umbral) |
| Importación/Exportación `.xlsx` | `ClosedXML` + `System.IO.Compression` | `MIT` | `ALLOW` | `EPPlus` (Licencia Polyform Noncommercial / Comercial) |
| Pasarela de Pagos | `Stripe.net` (SDK oficial C#) | `MIT` | `ALLOW` | — |
| Observabilidad y Logs | `Serilog` + `OpenTelemetry.Exporter.OpenTelemetryProtocol` | `Apache-2.0` | `ALLOW` | Agentes propietarios cerrados |
| Pruebas Automatizadas | `xunit` (`Microsoft.NET.Test.Sdk`) | `Apache-2.0` / `MIT` | `ALLOW` | — |

## 4. Consequences

- **Positive**:
  - Precisión matemática nativa con `decimal` (`MidpointRounding.AwayFromZero`) sin librerías externas en el núcleo de dominio (`Escandallo.Core`).
  - Cumplimiento garantizado de `license-policy.yaml` (cero dependencias `GPL`/`AGPL`/`SSPL`/`BSL`).
  - Despliegue atómico en un único contenedor ligero `.NET` conectado a PostgreSQL, reduciendo el coste operativo en el VPS Hetzner CX22 (`ADR-001`).
- **Negative / Trade-offs**:
  - La generación de archivos `.pdf` y `.xlsx` está acotada mediante `SemaphoreSlim(2, 2)` por centro (`SEC-REQ-DOS-001`) para garantizar uso predecible de memoria.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
| **1.1.0** | 2026-10-05 | Product Owner, agent-system-architect | Migración de stack de implementación a C# (.NET + System.Decimal + xUnit) | CHG-001-ESCANDALLO-MVP |
