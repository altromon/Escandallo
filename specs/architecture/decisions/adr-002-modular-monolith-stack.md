---
id: ADR-002-MODULAR-MONOLITH-STACK
type: architecture-decision-record
title: Arquitectura de Monolito Modular Full-Stack en .NET Puro (ASP.NET Core + Blazor Web App + PostgreSQL) Alineada con license-policy.yaml
status: proposed
version: 1.1.0
schema-version: '1.0'
deciders:
  - agent-system-architect
  - Product Owner
decision-date: '2026-10-06'
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


# ADR-002: Arquitectura de Monolito Modular Full-Stack en .NET Puro (ASP.NET Core + Blazor Web App + PostgreSQL) Alineada con license-policy.yaml

## 1. Context and Problem Statement

El sistema requiere una aplicación web accesible (WCAG 2.1 AA) y optimizada para PC, tablet y móvil (`QR-UI-A11Y-001`), un motor de cálculo financiero determinista (`FR-CALC-001`), generación de informes ejecutivos en PDF y Excel (`FR-REPORT-001`), integración con Stripe (`FR-BILLING-001`) y observabilidad avanzada (`FR-OBS-001`). Por decisión explícita de arquitectura se establece el uso de **.NET puro en todo el sistema (backend y frontend)** en lugar de TypeScript. Además, todo el código fuente debe cumplir las métricas estrictas de `quality-policy.yaml` (complejidad ciclomática $\le 10$, complejidad cognitiva $\le 15$, índice de mantenibilidad $\ge 50$, longitud de método $\le 40$ líneas) y el árbol de paquetes NuGet debe utilizar exclusivamente licencias permisivas aprobadas en `license-policy.yaml` (`MIT`, `Apache-2.0`, `BSD`, `PostgreSQL`).

## 2. Considered Technology Options

1. **Opción A (Elegida): Monolito Modular Hexagonal en .NET Puro (`C# 12/13` sobre `.NET 8/9 LTS` + `ASP.NET Core Minimal APIs` + `Blazor Web App SSR/Interactive` + `Npgsql/Dapper/EF Core` + `PostgreSQL 16`)**:
   - **Pros**: Un único ecosistema `.NET` compilado y fuertemente tipado de extremo a extremo (C# en dominio, API y vistas Blazor sin depender de TypeScript), tipo nativo `decimal` de 128 bits en base 10 para aritmética financiera exacta sin librerías externas, altísimo rendimiento de `Kestrel` con baja huella de memoria (`~90–150 MB RAM`), y ejecución paralelizada real en múltiples hilos de CPU para exportación documental.
   - **Licencias**: 100% `MIT` / `Apache-2.0` / `BSD-3-Clause` / `PostgreSQL`.
2. **Opción B: Arquitectura de Microservicios Distribuida (.NET API Gateway + Microservicios Separados + Broker RabbitMQ/Kafka)**:
   - **Contras**: Multiplica por $5\times$ el consumo de RAM y CPU, introduce latencia de serialización de red y transacciones distribuidas (Sagas) innecesarias para un dominio cuyas operaciones de recálculo son fuertemente consistentes (`BR-RECALC-HISTORY-001`). Viola el principio de simplicidad (YAGNI).
3. **Opción C: Stack Híbrido con Frontend SPA en TypeScript/Node.js o Librerías Documentales con Licencias Restrictivas (`iText` AGPL / `EPPlus v5+` Non-Commercial)**:
   - **Contras**: Introduce una segunda cadena de herramientas (Node.js/npm/TypeScript) rechazada por el propietario del producto, o incumple `license-policy.yaml` al incorporar licencias copyleft virales (`AGPL-3.0`) o comerciales restrictivas.

## 3. Decision Outcome

Se adopta la **Opción A (Monolito Modular Hexagonal en .NET Puro sobre .NET 8/9 LTS, ASP.NET Core, Blazor Web App y PostgreSQL 16)**, estructurado internamente en módulos de dominio desacoplados (`IamTenant`, `CostEngine`, `DocIo`, `BillingStripe`, `ObsTelemetry`) con fronteras de ensamblado/espacio de nombres estrictas.

### Catálogo de Paquetes NuGet Aprobados (`license-policy.yaml` Compliance)

| Capacidad | Paquete NuGet / Componente .NET Seleccionado | Licencia SPDX | Estado en `license-policy.yaml` | Alternativa Descartada (Motivo) |
| :--- | :--- | :---: | :---: | :--- |
| Servidor HTTP / API | `Microsoft.AspNetCore.App` (`Kestrel`, `Minimal APIs`, `RateLimiter`) | `MIT` | `ALLOW` | — |
| Interfaz Web Accesible | `Blazor Web App` (SSR + Interactive Server/Wasm) + CSS accesible WCAG 2.1 AA | `MIT` | `ALLOW` | Stacks basados en TypeScript/React (descartados por requerimiento de `.NET` puro) |
| Validación de Contratos | `System.ComponentModel.DataAnnotations` / `FluentValidation` | `MIT` / `Apache-2.0` | `ALLOW` | — |
| Aritmética Financiera | `System.Decimal` (`decimal` nativo C# de 128 bits base 10) | `MIT` | `ALLOW` | `double`/`float` IEEE 754 (error de coma flotante binaria) |
| Acceso a Datos SQL | `Npgsql` + `Dapper` / `Microsoft.EntityFrameworkCore.PostgreSQL` | `PostgreSQL` / `Apache-2.0` / `MIT` | `ALLOW` | — |
| Generación PDF | `PDFsharp` (`PdfSharpCore` / `PDFsharp-MigraDoc`) | `MIT` | `ALLOW` | `iText7` (`AGPL-3.0` bloqueada por `license-policy.yaml`) y `QuestPDF` (licencia comercial bajo umbral de ingresos) |
| Importación/Exportación `.xlsx` | `ClosedXML` + `System.IO.Compression.ZipArchive` + `XmlReader` seguro | `MIT` | `ALLOW` | `EPPlus v5+` (licencia Polyform Noncommercial no permitida) |
| Pasarela de Pagos | `Stripe.net` (SDK oficial de Stripe para .NET) | `MIT` | `ALLOW` | — |
| Observabilidad y Logs | `Serilog` + `OpenTelemetry.Exporter.OpenTelemetryProtocol` | `Apache-2.0` | `ALLOW` | Agentes APM propietarios cerrados |
| Testing BDD y Unitario | `xUnit` + `Reqnroll` (evolución OSS de SpecFlow) + `bUnit` | `Apache-2.0` / `BSD-3-Clause` / `MIT` | `ALLOW` | — |

## 4. Consequences

- **Positive**:
  - Stack 100% `.NET` puro de extremo a extremo con cumplimiento estricto de `license-policy.yaml` (cero dependencias `GPL`/`AGPL`/`SSPL`/`BSL` y cero dependencias de Node.js/TypeScript).
  - Despliegue atómico en un único contenedor Linux ligero (`mcr.microsoft.com/dotnet/aspnet:8.0-alpine`) conectado a PostgreSQL 16, reduciendo el coste operativo y aprovechando el multithreading nativo del CLR de .NET.
- **Negative / Trade-offs**:
  - La generación de archivos `.pdf` (`PDFsharp`) y `.xlsx` (`ClosedXML`) debe acotarse mediante `SemaphoreSlim` (máx. 2 exportaciones simultáneas por centro, `SEC-REQ-DOS-001`) y buffers en streaming (`MemoryStream` reciclado / `RecyclableMemoryStream`) para evitar presión sobre el *Large Object Heap (LOH)* del Garbage Collector de .NET.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
| **1.1.0** | 2026-10-06 | Product Owner, agent-system-architect | Migración arquitectónica completa a .NET Puro (ASP.NET Core + Blazor + System.Decimal) | ARCH-DOTNET-002 |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.1.0** | 2026-10-07 | agent-devops | Integración de incremento de entrega chg-001-escandallo-mvp | chg-001-escandallo-mvp |
