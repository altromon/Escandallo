---
id: ADR-005-DUAL-OBSERVABILITY-AND-SAFE-DOCUMENTS
type: architecture-decision-record
title: "Observabilidad Dual en Caliente con Redacción Inmutable (Serilog + OpenTelemetry) y Motor Documental 100% MIT (ClosedXML + PDFsharp)"
status: proposed
version: "1.1.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
decision-date: "2026-10-08"
affects-components:
  - CMP-DOC-IO-001
  - CMP-OBS-TELEMETRY-001
supersedes: null
superseded-by: null
---

# ADR-005: Observabilidad Dual en Caliente con Redacción Inmutable (Serilog + OpenTelemetry) y Motor Documental 100% MIT (ClosedXML + PDFsharp)

## 1. Contexto y Planteamiento del Problema

El sistema debe resolver dos retos críticos de arquitectura y cumplimiento normativo:
1. **Generación e Importación Segura de Documentos (`.xlsx` y `.pdf`) bajo `license-policy.yaml`**: En el ecosistema .NET, numerosas librerías populares de PDF y Excel (como *iText 7/8* bajo `AGPL-3.0`, *EPPlus v5+* bajo *Polyform Noncommercial*, o *QuestPDF* bajo licencia dual con límites comerciales) infringen la política corporativa de licencias (`DENY` para AGPL/GPL y rechazo de licencias comerciales propietarias no autorizadas). Además, la lectura de `.xlsx` está expuesta a Zip Bombs con cabeceras ZIP falsificadas (`ZipArchiveEntry.Length` spoofing), XXE y Formula Injection con espacios iniciales (`SEC-REQ-XLSX-001`), y la exportación concurrente puede fragmentar el *Large Object Heap (LOH)* (`SEC-REQ-DOS-001`).
2. **Observabilidad Dual en Caliente sin Fuga de PII ni Evicción de Alertas (`FR-OBS-001`, `SEC-REQ-OBS-001`)**: El administrador (`ACT-MAINTAINER-001`) necesita cambiar el nivel de log en tiempo de ejecución (`DEBUG`..`SECURITY`) sin reiniciar el servidor, garantizando que ni siquiera en modo `DEBUG` se filtren contraseñas en `Exception.Message`, tokens, firmas de Stripe ni datos personales, ni que una ráfaga de logs `INFO`/`DEBUG` sobrescriba las trazas `SECURITY` (`ABUSE-PRIV-OBS-001`).

## 2. Opciones Tecnológicas Consideradas

1. **Opción A (Recomendada — `ClosedXML` (`MIT`) + `PDFsharp` (`MIT`) + `Microsoft.IO.RecyclableMemoryStream` (`MIT`) con `BoundedReadStream` + `Serilog` (`Apache-2.0`) + `OpenTelemetry .NET` (`Apache-2.0`) con `LoggingLevelSwitch`, `PiiAndSecretRedactingPolicy` y Canal `SECURITY` Aislado)**:
   - Todas las librerías ostentan licencias `MIT` o `Apache-2.0` (`ALLOW` en `license-policy.yaml`).
2. **Opción B (`EPPlus` / `QuestPDF` / `iText` + Logging Estático con Reinicio)**:
   - Introduce bloqueos de auditoría de licencias (`agent-compliance-checker`) y obliga a reiniciar contenedores para diagnosticar incidencias en producción.

## 3. Decisión Adoptada

Se elige la **Opción A (`ClosedXML` MIT + `PDFsharp` MIT + `RecyclableMemoryStream` MIT + `Serilog` Apache-2.0 + `OpenTelemetry .NET` Apache-2.0)**.

**Justificación técnica y de cumplimiento (`SEC-REQ-XLSX-001`, `SEC-REQ-DOS-001`, `SEC-REQ-OBS-001`)**:
- **100% Conformidad SPDX (`license-policy.yaml`)**:
  - `ClosedXML` (`MIT`), `DocumentFormat.OpenXml` (`MIT`) y `Microsoft.IO.RecyclableMemoryStream` (`MIT`) para lectura/escritura segura de `.xlsx` sin fragmentar el LOH.
  - `PDFsharp` (`MIT`) para generación de dosieres ejecutivos `.pdf`.
  - `Serilog` (`Apache-2.0`) y `OpenTelemetry .NET` (`Apache-2.0`) para telemetría dual.
- **Blindaje Anti-Zip-Bomb (`BoundedReadStream`), Anti-XXE, Anti-DDE y LOH en `CMP-DOC-IO-001`**:
  - Antes de entregar el stream a `ClosedXML`, se valida tamaño comprimido $\le 2\text{ MB}$ (`0x504B0304`), $\le 64$ entradas ZIP, ratio declarado $\le 20:1$ y se descomprime a través de un `BoundedReadStream` sobre `RecyclableMemoryStream` que cuenta los bytes reales descomprimidos y aborta con `422 Unprocessable Entity` si superan `10 MB` (inmune a cabeceras ZIP falsificadas), aplicando `XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }` y cota de `5.000` filas.
  - Cualquier celda de texto cuyo valor tras `TrimStart()` comience por `=`, `+`, `-`, `@`, `\t` o `\r` se escapa con apóstrofo (`'`) y se asigna explícitamente como `XLDataType.Text`. Las exportaciones se limitan con `SemaphoreSlim(2, 2)`, `10 exportaciones/minuto` por centro y timeout de `10 s`.
- **Redacción Inmutable, Cota de Destructuración y Canal `SECURITY` en `CMP-OBS-TELEMETRY-001`**:
  - `Serilog.Core.LoggingLevelSwitch` conmuta niveles en `< 1000 ms`, limitando la destructuración con `Destructuring.ToMaximumDepth(3)` y `ToMaximumStringLength(1024)`.
  - `PiiAndSecretRedactingPolicy` reemplaza secretos y PII por `[REDACTED]` tanto en propiedades estructuradas como en cadenas de conexión `Npgsql` (`Password=...`) y claves `whsec_` dentro de `Exception.Message`, serializando en JSON de una sola línea (escapando `\r`, `\n`, `\t`) y aislando los eventos `SECURITY` en un canal inmutable protegido contra evicción.

## 4. Consecuencias

- **Positivas**:
  - Aprobación garantizada en el gate de licencias (`verify_licenses`) y en la auditoría de seguridad (`verify_security`).
  - Huella de memoria predecible sin fragmentación de LOH gracias a `SemaphoreSlim(2, 2)` y `RecyclableMemoryStream`.
- **Negativas / Compensaciones (Trade-offs)**:
  - `PDFsharp` requiere maquetación programática mediante coordenadas/tablas MigraDoc (`MIT`), lo cual se encapsula limpiamente en `IExecutiveReportExporter`.

---

## 5. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Decisores | Estado de la Decisión | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-04-12 | `agent-system-architect` | Propuesta formal aceptada | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Incorporación de `BoundedReadStream`, `RecyclableMemoryStream`, `XLDataType.Text`, `ToMaximumDepth(3)`, redacción en `Exception.Message` y canal `SECURITY` anti-evicción | `chg-001-escandallo-mvp` |
