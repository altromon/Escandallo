---
id: SEC-REQ-XLSX-001
type: security-requirement
title: "Parseo Seguro de Plantillas Excel Anti-XXE/ZipBomb y Neutralización de Fórmulas DDE"
status: draft
version: "1.1.0"
schema-version: "1.0"
category: security
priority: high
derives-from:
  - UC-COST-CATALOG-001
  - UC-EXEC-REPORT-001
mitigates-abuse-case:
  - ABUSE-XLSX-INJECT-001
enforced-in-enclave: "SEC-ENC-APP-CORE-001"
owasp-asvs: "V5.2, V5.3, V12.1"
stride-category: "Tampering"
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@SEC-REQ-XLSX-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Security Requirement: Parseo Seguro de Plantillas Excel Anti-XXE/ZipBomb y Neutralización de Fórmulas DDE

## 1. Normative Statement

Para mitigar `ABUSE-XLSX-INJECT-001` (OWASP ASVS V5.2, V5.3, V12.1), el procesamiento de archivos `.xlsx` en `CMP-DOC-IO-001` (`ZipArchive` + `ClosedXML`) y la validación aritmética en `CMP-COST-ENGINE-001` (`System.Decimal`) dentro de `SEC-ENC-APP-CORE-001` **DEBEN** cumplir:
1. **Defensa contra Zip Bombs (Incluido Spoofing de Cabeceras ZIP) y XXE en Importación**: El tamaño máximo del archivo `.xlsx` subido **DEBE** limitarse a `2 MB` validando los *magic bytes* `0x504B0304`, máximo `64` entradas ZIP, ratio declarado $\le 20:1$ y lectura a través de un stream contador acotado (`BoundedReadStream` sobre `RecyclableMemoryStream`) que aborte inmediatamente si los bytes reales descomprimidos superan `10 MB` (incluso si `ZipArchiveEntry.Length` fue falsificado), con `XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }` y cota de `5.000` filas.
2. **Neutralización de Inyección de Fórmulas DDE en `ClosedXML` (`TrimStart`)**: Al importar o exportar cualquier celda de texto cuyo valor (o su versión tras `TrimStart()`) comience por `=`, `+`, `-`, `@`, tabulador (`0x09`) o retorno de carro (`0x0D`), el sistema **DEBE** anteponer un apóstrofo de escape (`'`) y asignar explícitamente el tipo de celda como literal de texto (`XLDataType.Text`) en `ClosedXML`.
3. **Guardas de Rango y Prevención de `OverflowException` en `System.Decimal` (128-bit)**: El motor de validación **DEBE** rechazar cualquier entrada no numérica (`NaN`/`Infinity`), negativa, divisor igual a `0` o valor que exceda las cotas operativas (`PurchasePrice > 1_000_000m`, `UnitsPerPackage < 0.0001m`, `WastePct > 100m`) antes de ejecutar multiplicaciones en `System.Decimal` o persistir en `NUMERIC(18,6)`.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@SEC-REQ-XLSX-001 @automated @regression @security @mitigation
Feature: Protección contra XXE, bombas ZIP e inyección de fórmulas DDE en archivos Excel
  Como componente de importación y generación documental (SEC-ENC-APP-CORE-001)
  Quiero inspeccionar la estructura OOXML, contar los bytes reales descomprimidos y escapar prefijos ejecutables
  Para proteger al servidor .NET 10 contra agotamiento de memoria/XXE/OverflowException y al cliente contra ejecución de comandos en Excel

  Scenario: Rechazo de plantilla Excel con entidad externa XML (XXE) o ratio de compresión malicioso
    Given un usuario autenticado sube un archivo ".xlsx" al importador de catálogo del centro "C_PROPIO"
    When el parser detecta una declaración DOCTYPE XML externa o un tamaño descomprimido superior a 10 MB
    Then el sistema aborta la lectura inmediatamente y devuelve estado HTTP 422
    And no se persiste ningún registro de coste en la base de datos
    And se registra un evento con severidad "SECURITY" detallando el vector bloqueado

  Scenario Outline: Neutralización de payloads de inyección de fórmulas DDE y valores aritméticos inválidos o en desbordamiento
    Given el usuario intenta registrar o importar un ítem de catálogo con nombre "<nombre_input>" y coste "<valor_numerico>"
    When el sistema procesa la entrada y genera posteriormente la exportación del informe ejecutivo en ".xlsx"
    Then el sistema aplica la acción de seguridad "<accion_esperada>" con resultado "<estado_resultado>"

    Examples:
      | nombre_input                         | valor_numerico | accion_esperada                         | estado_resultado |
      | =CMD\|'/C calc'!A0                   | 25.00          | escapar_prefijo_como_texto_literal      | SANITIZADO       |
      | +HYPERLINK("http://evil.tld")        | 18.50          | escapar_prefijo_como_texto_literal      | SANITIZADO       |
      | @SUM(1+1)*cmd                        | 12.00          | escapar_prefijo_como_texto_literal      | SANITIZADO       |
      | Crema Hidratante Facial              | NaN            | rechazar_entrada_aritmetica_invalida    | HTTP_422         |
      | Vial Ácido Hialurónico               | -15.00         | rechazar_entrada_aritmetica_invalida    | HTTP_422         |
      | Aparatología Láser Diodo             | 99999999999.99 | rechazar_desbordamiento_decimal_128bit  | HTTP_422         |

  Scenario Outline: Análisis de frontera en tamaño comprimido (2MB), descomprimido real en BoundedReadStream (10MB), ratio ZIP (20:1) y filas (5000)
    Given un archivo ".xlsx" tiene tamaño comprimido <tam_comprimido_kb> KB, bytes reales descomprimidos en BoundedReadStream <tam_descomprimido_kb> KB, ratio <ratio_zip> y <filas> filas
    When el validador de archivos procesa la subida en la frontera de seguridad
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | tam_comprimido_kb | tam_descomprimido_kb | ratio_zip | filas | codigo_http |
      | 2047              | 10239                | 19        | 4999  | 200         |
      | 2048              | 10240                | 20        | 5000  | 200         |
      | 2049              | 5000                 | 5         | 100   | 422         |
      | 1000              | 10241                | 11        | 100   | 422         |
      | 500               | 9000                 | 21        | 100   | 422         |
      | 500               | 2000                 | 4         | 5001  | 422         |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial de seguridad para parseo y generación de archivos Excel. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Auditoría técnica .NET 10 LTS: stream contador `BoundedReadStream` contra cabeceras ZIP falsas, `XLDataType.Text` en `ClosedXML` y cota anti-`OverflowException` en `System.Decimal`. |
