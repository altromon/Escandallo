---
id: ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS
type: architecture-decision-record
title: Observabilidad Dual Ligera (Serilog + OpenTelemetry .NET) con Nivel en Caliente y Motor Documental Seguro (ClosedXML / PDFsharp)
status: proposed
version: 1.1.0
schema-version: '1.0'
deciders:
  - agent-system-architect
  - agent-threat-modeler
decision-date: '2026-10-06'
affects-components:
  - CMP-DOC-IO-001
  - CMP-OBS-TELEMETRY-001
supersedes: null
superseded-by: null
---


# ADR-005: Observabilidad Dual Ligera (Serilog + OpenTelemetry .NET) con Nivel en Caliente y Motor Documental Seguro (ClosedXML / PDFsharp)

## 1. Context and Problem Statement

Dos requisitos técnicos clave impactan directamente en el consumo de memoria del servidor `.NET` y en la superficie de seguridad:
1. **Observabilidad Dual y Nivel en Caliente (`FR-OBS-001`, `SEC-REQ-OBS-001`)**: El usuario mantenedor (`ACT-MAINTAINER-001`) debe poder extraer logs, trazas y métricas tanto desde una consola web integrada (`JSON`, `CSV`, `OTLP`) como hacia un stack externo, cambiando en caliente la severidad (`DEBUG`, `INFO`, `WARN`, `ERROR`, `SECURITY`) sin reiniciar el proceso ASP.NET Core y sin exponer jamás credenciales o PII en modo `DEBUG`.
2. **Importación/Exportación Documental (`FR-REPORT-001`, `SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`)**: El sistema importa catálogos desde plantillas `.xlsx` (`M4`) y exporta informes ejecutivos exclusivamente en `.pdf` y `.xlsx`, debiendo resistir ataques XXE, Bombas ZIP, inyección de fórmulas DDE y agotamiento de RAM por concurrencia.

## 2. Considered Technology Options

1. **Opción A (Elegida): Telemetría con `Serilog` (`LoggingLevelSwitch`) + `OpenTelemetry .NET` sobre Partición Circular en PostgreSQL + Exportadores OTLP, y Motor Documental Nativo en .NET (`PDFsharp` + `ClosedXML`)**:
   - **Telemetría Dual Sin Sobrecoste RAM**: En lugar de desplegar Elasticsearch/Logstash/Kibana (que consumiría $>6\text{ GB RAM}$), `CMP-OBS-TELEMETRY-001` utiliza `Serilog` (`Apache-2.0`) con un `IDestructuringPolicy` de redactado automático en origen (`[REDACTED]`) y `LoggingLevelSwitch` para conmutación atómica de nivel en caliente en $< 5\text{ ms}`. Escribe asíncronamente en una tabla particionada/rotativa (`telemetry_events`, retención configurable 30 días) para alimentar al instante la consola web del mantenedor (`JSON`/`CSV`/`OTLP`), a la vez que emite por protocolo estándar OTLP/HTTP (`OpenTelemetry.Exporter.OpenTelemetryProtocol`) hacia colectores externos.
   - **Motor Documental Nativo (`PDFsharp` + `ClosedXML`)**: Genera PDFs vectoriales con `PDFsharp` (`MIT`) y hojas Excel con `ClosedXML` (`MIT`) sobre flujos acotados, aplicando un `SemaphoreSlim` por centro de máximo 2 exportaciones concurrentes.
2. **Opción B: Stack ELK Autohospedado + Renderizado PDF con Headless Chromium (`Playwright` / `PuppeteerSharp`) o Librerías Copyleft (`iText7` AGPL)**:
   - **Contras**: Un solo proceso de Chromium consume 300–600 MB de RAM por PDF generado, `iText7` viola `license-policy.yaml` (`AGPL-3.0`), y ELK requiere un servidor dedicado de alto coste, destruyendo la rentabilidad de `ADR-001`.

## 3. Decision Outcome

Se elige la **Opción A (Telemetría Dual Ligera en .NET con Redactado en Origen + Generación Documental Nativa con `PDFsharp`/`ClosedXML` y Pre-Inspección Anti-XXE/ZipBomb)**.

### Controles de Seguridad Integrados

1. **Pre-Inspección de Archivos `.xlsx` (`CMP-DOC-IO-001` — `SEC-REQ-XLSX-001`)**:
   - Antes de abrir el flujo con `ClosedXML`, un validador basado en `System.IO.Compression.ZipArchive` y `XmlReader` (`DtdProcessing = DtdProcessing.Prohibit`, `XmlResolver = null`) inspecciona el directorio central del archivo (máx. `2 MB` comprimido): si la suma de `entry.Length` descomprimidos supera `10 MB` o el ratio de compresión supera `20:1`, o si algún flujo XML contiene declaraciones `<!DOCTYPE` / `<!ENTITY`, la petición se aborta inmediatamente con `HTTP 422` y alerta `SECURITY`.
   - Toda celda de texto en importación o exportación que comience por `=`, `+`, `-`, `@`, `\t` o `\r` es neutralizada anteponiendo `'` y forzando `XLDataType.Text`.
2. **Inmutabilidad de Redactado en `DEBUG` (`CMP-OBS-TELEMETRY-001` — `SEC-REQ-OBS-001`)**:
   - El pipeline de `Serilog` aplica un `SecretRedactingDestructuringPolicy` y `LogEventEnricher` que enmascara con `[REDACTED]` las claves `Authorization`, `Cookie`, `password`, `token`, `secret`, `Stripe-Signature` e `iban` como una capa inmutable independiente del valor actual de `LoggingLevelSwitch`.

## 4. Consequences

- **Positive**:
  - Permite ejecutar toda la suite de observabilidad y generación de informes PDF/Excel en `.NET` puro dentro de la huella de `< 512 MB RAM` por proceso definida en las restricciones de arquitectura, con licencias 100% `MIT`/`Apache-2.0`.
- **Negative / Trade-offs**:
  - El diseño visual del informe PDF se maqueta programáticamente mediante primitivas de tablas y gráficos vectoriales en `PDFsharp` en lugar de imprimir HTML/CSS con un navegador headless.

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
| **1.1.0** | 2026-10-06 | Product Owner, agent-system-architect | Migración a Serilog, OpenTelemetry .NET, ClosedXML y PDFsharp | ARCH-DOTNET-002 |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.1.0** | 2026-10-07 | agent-devops | Integración de incremento de entrega chg-001-escandallo-mvp | chg-001-escandallo-mvp |
