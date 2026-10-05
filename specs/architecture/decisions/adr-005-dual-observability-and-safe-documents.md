---
id: ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS
type: architecture-decision-record
title: "Observabilidad Dual Ligera (Pino + OpenTelemetry) con Nivel en Caliente y Motor Documental Seguro en Streaming (ExcelJS / PDFKit)"
status: proposed
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "agent-threat-modeler"
decision-date: "2026-10-05"
affects-components:
  - CMP-DOC-IO-001
  - CMP-OBS-TELEMETRY-001
supersedes: null
superseded-by: null
---

# ADR-005: Observabilidad Dual Ligera (Pino + OpenTelemetry) con Nivel en Caliente y Motor Documental Seguro en Streaming (ExcelJS / PDFKit)

## 1. Context and Problem Statement

Dos requisitos técnicos clave impactan directamente en el consumo de memoria del servidor y en la superficie de seguridad:
1. **Observabilidad Dual y Nivel en Caliente (`FR-OBS-001`, `SEC-REQ-OBS-001`)**: El usuario mantenedor (`ACT-MAINTAINER-001`) debe poder extraer logs, trazas y métricas tanto desde una consola web integrada (`JSON`, `CSV`, `OTLP`) como hacia un stack externo, cambiando en caliente la severidad (`DEBUG`, `INFO`, `WARN`, `ERROR`, `SECURITY`) sin reiniciar el servidor y sin exponer jamás credenciales o PII en modo `DEBUG`.
2. **Importación/Exportación Documental (`FR-REPORT-001`, `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`)**: El sistema importa catálogos desde plantillas `.xlsx` (`M4`) y exporta informes ejecutivos exclusivamente en `.pdf` y `.xlsx`, debiendo resistir ataques XXE, Bombas ZIP, inyección de fórmulas DDE y agotamiento de RAM por concurrencia.

## 2. Considered Technology Options

1. **Opción A (Elegida): Telemetría con `Pino` + `OpenTelemetry` sobre Partición Circular en PostgreSQL + Exportadores OTLP, y Motor Documental Nativo en Streaming (`PDFKit` + `ExcelJS`)**:
   - **Telemetría Dual Sin Sobrecoste RAM**: En lugar de desplegar Elasticsearch/Logstash/Kibana (que consumiría $>6\text{ GB RAM}$), `CMP-OBS-TELEMETRY-001` utiliza `pino` (`MIT`) con redactado automático en origen (`censor: '[REDACTED]'`) y escribe asíncronamente en una tabla particionada/rotativa (`telemetry_events`, retención configurable 30 días) para alimentar al instante la consola web del mantenedor (`JSON`/`CSV`/`OTLP`), a la vez que emite por protocolo estándar OTLP/HTTP hacia colectores externos (ej. Grafana Cloud Free Tier). El nivel de log reside en una variable atómica en memoria sincronizada con PostgreSQL, permitiendo cambios en caliente en $< 10\text{ ms}`.
   - **Motor Documental Nativo (`PDFKit` + `ExcelJS`)**: Genera PDFs vectoriales con `pdfkit` (`MIT`) y hojas Excel con `exceljs` (`MIT`) en streaming (~5–15 MB de RAM por documento), aplicando un semáforo en memoria de máximo 2 exportaciones concurrentes por centro.
2. **Opción B: Stack ELK / Loki Autohospedado + Renderizado PDF con Headless Chromium (`Puppeteer` / `Playwright`)**:
   - **Contras**: Un solo proceso de Chromium consume 300–600 MB de RAM por PDF generado, y ELK requiere un servidor dedicado de alto coste, destruyendo la rentabilidad de `ADR-001` y facilitando ataques DoS (`ABUSE-DOS-CASCADE-001`).

## 3. Decision Outcome

Se elige la **Opción A (Telemetría Dual Ligera con Redactado en Origen + Generación Documental Nativa con Pre-Inspección Anti-XXE/ZipBomb)**.

### Controles de Seguridad Integrados

1. **Pre-Inspección de Archivos `.xlsx` (`CMP-DOC-IO-001` — `SEC-REQ-XLSX-001`)**:
   - Antes de pasar el buffer a `exceljs`, un validador de cabeceras ZIP inspecciona el directorio central del archivo (máx. `2 MB` comprimido): si la suma de tamaños descomprimidos de las entradas XML supera `10 MB` o el ratio de compresión supera `20:1`, o si algún flujo XML contiene declaraciones `<!DOCTYPE` / `<!ENTITY`, la petición se aborta inmediatamente con `HTTP 422` y alerta `SECURITY`.
   - Toda celda de texto en importación o exportación que comience por `=`, `+`, `-`, `@`, `\t` o `\r` es neutralizada anteponiendo `'` y forzando tipo literal `string`.
2. **Inmutabilidad de Redactado en `DEBUG` (`CMP-OBS-TELEMETRY-001` — `SEC-REQ-OBS-001`)**:
   - El serializador de `pino` configura `redact: { paths: ['req.headers.authorization', 'req.headers.cookie', '*.password', '*.token', '*.secret', '*.stripeSignature', '*.iban'], censor: '[REDACTED]' }` como una capa inmutable independiente del nivel de severidad activo.

## 4. Consequences

- **Positive**:
  - Permite ejecutar toda la suite de observabilidad y generación de informes PDF/Excel dentro de la huella de `< 512 MB RAM` por proceso definida en las restricciones de arquitectura.
- **Negative / Trade-offs**:
  - El diseño visual del informe PDF se maqueta programáticamente mediante primitivas de tablas y gráficos vectoriales en `PDFKit` en lugar de imprimir HTML/CSS con un navegador headless.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
