---
id: CMP-DOC-IO-001
type: component
title: "Subsistema de Importación Segura de Catálogos (.xlsx) y Exportación de Informes Ejecutivos (PDF / Excel)"
status: proposed
version: "1.0.0"
schema-version: "1.0"
level: 2
bounded-context: "Document Ingestion & Executive Reporting"
parent-component: CMP-ESCANDALLO-SYS-001
implementation-type: service
implements-use-cases:
  - UC-COST-CATALOG-001
  - UC-EXEC-REPORT-001
satisfies-requirements:
  - FR-COST-001
  - FR-REPORT-001
  - SEC-REQ-XLSX-001
  - SEC-REQ-DOS-001
  - SEC-REQ-TENANT-001
hosted-in-enclave: SEC-ENC-APP-CORE-001
interfaces:
  - name: "Document Import (.xlsx) & Executive Export (.pdf, .xlsx) API"
    protocol: "REST/HTTP Streaming"
    contract-spec: "specs/security/requirements/sec-req-xlsx-001.md"
supersedes: null
superseded-by: null
---

# CMP-DOC-IO-001: Subsistema de Importación Segura de Catálogos (.xlsx) y Exportación de Informes Ejecutivos (PDF / Excel)

## 1. Purpose, Responsibility, and Bounded Context

Subsistema de Nivel 2 (`ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS`) dedicado al procesamiento seguro de archivos de hoja de cálculo y generación de informes ejecutivos:
1. **Onboarding por Catálogo Semilla e Importación Segura `.xlsx` (`FR-COST-001`, `SEC-REQ-XLSX-001`, M4)**: Carga en 1 clic del catálogo semilla preconfigurado o importación mediante plantilla `.xlsx` con pre-inspección estructural del contenedor ZIP/OOXML (`<= 2 MB` comprimido, `<= 10 MB` descomprimido, ratio máx. `20:1`, `<= 5.000` filas, DTDs/XXE deshabilitados) y neutralización de fórmulas DDE (`=`, `+`, `-`, `@`).
2. **Generación de Informes Ejecutivos en `.pdf` y `.xlsx` (`FR-REPORT-001`, M2)**: Construye informes ejecutivos filtrables por estado (`active` / `archived`, M1) y por nivel de riesgo de margen (`🟢/🟡/🔴`, M2), incluyendo el desglose completo de las 5 categorías de coste, `Beneficio Neto (EUR)`, `Precio sin IVA`, `PVP Calculado` vs `PVP Comercial Fijado` y evolución histórica.

## 2. Structure and Connectivity Diagram (arc42 Sec. 5 / NAF v4)

```mermaid
graph TD
    IAM["CMP-IAM-TENANT-001 (Semáforo <= 2 Concurrentes)"] --> Doc["CMP-DOC-IO-001"]
    Doc -->|Inspección ZIP/XML Anti-XXE/ZipBomb| Validator["Validador OOXML & Sanitizador DDE"]
    Doc -->|Streaming PDFKit (MIT)| PDF["Generador Informe .pdf"]
    Doc -->|Streaming ExcelJS (MIT)| XLSX["Generador Informe .xlsx"]
    Doc -->|Lectura RLS por center_id| DB[("PostgreSQL 16")]
```

## 3. Interface Contracts and Execution Policies

- **Execution Mechanism**: Generación en streaming sobre buffers acotados (`pdfkit` y `exceljs`) protegida por un semáforo de concurrencia por centro (`máx. 2 exportaciones simultáneas`, `10/minuto`, timeout duro de `10 segundos`, `SEC-REQ-DOS-001`).
- **Fault Tolerance and Performance**: Consumo de memoria por exportación $< 15\text{ MB RAM}$; aborto inmediato ante anomalías de compresión o entidades XML externas.

---

## 4. Revision History and Version Control

| Version | Date | Author / Agent | Change Description | Change Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Definición inicial del subsistema de importación y exportación documental | ARCH-INIT-001 |
