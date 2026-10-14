---
id: ARCH-CONSTR-001
type: architecture-constraints
title: "02. Restricciones de Arquitectura (arc42 Sec. 2 / NAF Constraints)"
status: proposed
version: "1.1.0"
schema-version: "1.0"
arc42-section: 2
naf-perspective: "Architecture Constraints"
constraints:
  - CON-TECH-DOTNET-001
  - CON-TECH-DECIMAL-002
  - CON-TECH-FINOPS-003
  - CON-TECH-A11Y-004
  - ACON-ORG-SDD-001
  - ACON-ORG-HUMAN-002
  - ACON-SEC-TENANT-003
license-policy: "license-policy.yaml"
supersedes: null
superseded-by: null
---

# 02. Restricciones de Arquitectura (arc42 Sec. 2 / NAF Constraints)

## 1. Restricciones Técnicas Innegociables (`CON-*`)

Limitaciones impuestas por mandato tecnológico del Product Owner, precisión financiera, rendimiento en contenedores y seguridad perimetral:

| ID Restricción | Nombre de la Restricción | Descripción y Justificación Técnica |
| :--- | :--- | :--- |
| `CON-TECH-DOTNET-001` | **Stack Nativo .NET 10 LTS + UI en Blazor** | Todo el sistema (backend, motor de dominio e interfaz de usuario) debe construirse de forma nativa sobre **.NET 10 LTS (C# 14)** y **ASP.NET Core Blazor Web App**, evitando arquitecturas fragmentadas con múltiples runtimes (Node/Python) en producción (`ADR-002-MODULAR-MONOLITH-STACK`). |
| `CON-TECH-DECIMAL-002` | **Prohibición de Coma Flotante Binaria y Guardas de Rango en Finanzas** | Queda estrictamente prohibido el uso de `float` o `double` (IEEE 754) en cálculos de costes, mermas, amortizaciones, impuestos o márgenes. Es obligatorio el uso exclusivo de `System.Decimal` (128 bits) con guardas de rango anti-`OverflowException` (`PurchasePrice <= 1_000_000m`, `UnitsPerPackage >= 0.0001m`, `WastePct <= 100m`) en C# y `NUMERIC(18,6)` en PostgreSQL (`BR-CALC-FORMULAS-001`, `SEC-REQ-XLSX-001`, `ADR-004-EXACT-DECIMAL-AND-CASCADE-ENGINE`). |
| `CON-TECH-FINOPS-003` | **Techo Presupuestario y Acotación de Memoria Anti-DoS (`<= 2 GB` RAM)** | La topología de despliegue inicial debe ejecutarse dentro de una huella de memoria controlada ($\le 2\text{ GB}$ RAM para la aplicación + $\le 3\text{ GB}$ RAM para PostgreSQL) en un nodo Cloud VPS europeo con coste total $< 12\text{ EUR/mes}$, imponiendo cotas de circuitos Blazor SignalR ($\le 5$ por usuario, $\le 32\text{ KB}$ por mensaje), `SemaphoreSlim(2, 2)` y `RecyclableMemoryStream` contra fragmentación de LOH (`SEC-REQ-DOS-001`, `ADR-001-COST-EFFECTIVE-DEPLOYMENT`). |
| `CON-TECH-A11Y-004` | **Cumplimiento Estricto WCAG 2.1 AA y Mobile-First** | La interfaz Blazor debe renderizarse sin desplazamiento horizontal desde `360px` hasta `4K`, garantizando navegación por teclado, roles ARIA semánticos y contraste mínimo `4.5:1` (`QR-UI-A11Y-001`). |

---

## 2. Restricciones Organizativas y de Proceso (`ACON-*`)

Directivas de gobernanza AI-SDLC, soberanía humana y cumplimiento normativo europeo (RGPD / OWASP ASVS v4.0.3 Nivel 2):

| ID Restricción | Nombre | Directiva Vinculante |
| :--- | :--- | :--- |
| `ACON-ORG-SDD-001` | **Gobernanza SDD y Calidad Determinista** | Todo código fuente en `src/` debe mantener trazabilidad 360° con requisitos (`FR-*`, `QR-*`, `SEC-REQ-*`) y respetar los umbrales de `quality-policy.yaml`: Complejidad Ciclomática $\le 10$, Complejidad Cognitiva $\le 15$, Índice de Mantenibilidad $\ge 50$, y longitud máxima de función $\le 40$ líneas. |
| `ACON-ORG-HUMAN-002` | **Soberanía Humana Inalienable** | Prohibición absoluta de auto-aprobación o auto-fusión desatendida de Pull Requests hacia ramas estables (`main`, `release/*`). |
| `ACON-SEC-TENANT-003` | **Residencia de Datos en la UE, RLS Transaccional y Privacidad por Diseño** | Todos los datos de centros estéticos y telemetría deben residir en centros de datos dentro de la Unión Europea (RGPD), con cifrado TLS 1.3 / mTLS en tránsito, LUKS/AES-256 en reposo, conexión PostgreSQL sin `BYPASSRLS` (`SET LOCAL`), y redacción obligatoria (`[REDACTED]`) de PII y secretos tanto en propiedades como en `Exception.Message` con canal `SECURITY` inmutable (`SEC-REQ-TENANT-001`, `SEC-REQ-OBS-001`). |

---

## 3. Cumplimiento de Licencias Open Source (`LIC-POL-*`)

De conformidad estricta con [`license-policy.yaml`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/license-policy.yaml):
- **Permisivas Permitidas (`ALLOW`)**: `MIT`, `Apache-2.0`, `BSD-2-Clause`, `BSD-3-Clause`, `ISC`.
- **Copyleft Débil (`REVIEW_REQUIRED`)**: `LGPL-2.1`, `LGPL-3.0`, `MPL-2.0` (requieren revisión humana explícita).
- **Copyleft Fuerte / Virales o Comerciales Restrictivas (`DENY`)**: `GPL-2.0`, `GPL-3.0`, `AGPL-3.0`.
  - **Impacto Arquitectónico Directo**: Quedan **vetadas** en el ecosistema .NET librerías de generación de documentos con licencias duales AGPL/Comerciales o con umbrales de facturación restrictivos (como *iText 7/8* bajo AGPLv3, *QuestPDF* bajo licencia dual con límite de ingresos o *EPPlus* v5+ bajo Polyform Noncommercial). Se selecciona exclusivamente **`ClosedXML` (`MIT`)** para Excel, **`PDFsharp` (`MIT`)** para PDF y **`Microsoft.IO.RecyclableMemoryStream` (`MIT`)** para gestión de buffers sin presión en el LOH (`ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS`).

---

## 4. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-04-12 | `agent-system-architect` | Definición inicial de restricciones técnicas, organizativas y de licencias SPDX | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Incorporación de restricciones técnicas de ciberseguridad v1.1.0 (`SEC-REQ-*` sobre `System.Decimal`, LOH, circuitos Blazor y `NOBYPASSRLS`) | `chg-001-escandallo-mvp` |
