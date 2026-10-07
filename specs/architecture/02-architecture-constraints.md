---
id: ARCH-CONSTR-001
type: architecture-constraints
title: "02. Architecture Constraints"
status: proposed
version: "1.0.0"
schema-version: "1.0"
arc42-section: 2
naf-perspective: "Architecture Constraints"
constraints:
  - CON-TECH-001
  - CON-TECH-002
  - CON-TECH-003
  - ACON-ORG-001
license-policy: "license-policy.yaml"
supersedes: null
superseded-by: null
---

# 02. Architecture Constraints (arc42 Sec. 2 / NAF Constraints)

## 1. Non-Negotiable Technical Constraints (`CON-*`)

| Constraint ID | Constraint Name | Description and Technical Rationale |
| :--- | :--- | :--- |
| `CON-TECH-001` | **Ejecución Server-Side Multi-Cliente con Coste Mínimo** | La lógica de negocio, validación de cuota gratuita (2 escandallos activos) y persistencia deben residir en servidor, manteniendo un TCO inicial $< 10\text{ EUR/mes}$ (`ADR-001-COST-EFFECTIVE-DEPLOYMENT`). |
| `CON-TECH-002` | **Runtime y Toolchain Determinista en .NET Puro** | Ejecución exclusiva sobre `.NET 8/9 LTS` (`C# 12/13` con `Nullable=enable`), `ASP.NET Core Minimal APIs`, `Blazor Web App` y base de datos relacional PostgreSQL 16 (`ADR-002-MODULAR-MONOLITH-STACK`), sin dependencia de TypeScript. |
| `CON-TECH-003` | **Huella de Memoria Acotada (`<= 512 MB`)** | El contenedor del servidor de aplicación `.NET` no debe superar los `512 MB RAM` bajo carga nominal, acotando a 2 las exportaciones PDF/Excel simultáneas por centro (`SEC-REQ-DOS-001`). |
| `CON-TECH-004` | **Formatos de Exportación Exclusivos** | Los informes ejecutivos (`FR-REPORT-001`) se exportan única y exclusivamente en **PDF (`.pdf`)** y **Excel (`.xlsx`)**. |

---

## 2. Organizational and Quality Constraints (`ACON-*`)

| Constraint ID | Name | Binding Directive |
| :--- | :--- | :--- |
| `ACON-ORG-001` | **Umbrales Estrictos de Código (`quality-policy.yaml`)** | Complejidad ciclomática $\le 10$, complejidad cognitiva $\le 15$, índice de mantenibilidad $\ge 50.0$ y longitud máxima de método $\le 40$ líneas. |
| `ACON-ORG-002` | **Trazabilidad 360° PDaC y Soberanía Humana** | Todo requisito (`FR-*`, `QR-*`, `SEC-REQ-*`) debe estar trazado a un componente `CMP-*` y verificado por escenarios BDD Gherkin antes de pasar a producción. |

---

## 3. Open Source License Compliance (`LIC-POL-*`)

En cumplimiento estricto de [`license-policy.yaml`](../../license-policy.yaml):
- **Permisivas (`ALLOW`)**: `MIT`, `Apache-2.0`, `BSD-2-Clause`, `BSD-3-Clause`, `ISC`, `Unlicense`, `CC0-1.0`, `0BSD`. Todas las dependencias NuGet de producción elegidas (`Microsoft.AspNetCore.App`, `Blazor`, `Npgsql`, `Dapper`, `PDFsharp`, `ClosedXML`, `Serilog`, `Stripe.net`, `OpenTelemetry`) pertenecen a esta categoría.
- **Copyleft Débil (`REVIEW_REQUIRED`)**: `LGPL-2.1-only`, `LGPL-3.0-only`, `MPL-2.0` (evitadas en el núcleo).
- **Copyleft Fuerte / Viral (`DENY`)**: `GPL-2.0`, `GPL-3.0`, `AGPL-3.0` (estrictamente bloqueadas; se descartan librerías PDF como `iText7` bajo licencia AGPL).
- **Comerciales (`COMMERCIAL_APPROVAL_REQUIRED`)**: `SSPL-1.0`, `BSL-1.1` o licencias no comerciales (`EPPlus v5+`, `QuestPDF` comercial) descartadas en favor de `ClosedXML` (`MIT`) y `PDFsharp` (`MIT`).

---

## 4. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial de restricciones técnicas, de calidad y de licencias | ARCH-INIT-001 |
