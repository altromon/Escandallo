---
id: SEC-REQ-XLSX-001
type: security-requirement
title: "Parseo Seguro de Plantillas Excel Anti-XXE/ZipBomb y Neutralización de Fórmulas DDE"
status: draft
version: "1.0.0"
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

Para mitigar `ABUSE-XLSX-INJECT-001` (OWASP ASVS V5.2, V5.3, V12.1), el procesamiento de archivos `.xlsx` tanto en importación de catálogos (`M4` en `UC-COST-CATALOG-001`) como en exportación de informes ejecutivos (`UC-EXEC-REPORT-001`) **DEBE** cumplir:
1. **Defensa contra Zip Bombs y XXE en Importación**: El tamaño máximo del archivo `.xlsx` subido **DEBE** limitarse a `2 MB`, el tamaño máximo descomprimido en memoria a `10 MB` (con ratio de compresión máximo `20:1` y límite de `5.000` filas), validando los *magic bytes* ZIP/OOXML y deshabilitando por completo la resolución de DTDs y entidades externas XML (`disallow-doctype-decl = true`, ` resolveExternalEntities = false`).
2. **Neutralización de Inyección de Fórmulas (CSV/Excel DDE)**: Al importar o exportar cualquier celda de texto cuyo valor comience por los caracteres `=`, `+`, `-`, `@`, tabulador (`0x09`) o retorno de carro (`0x0D`), el sistema **DEBE** rechazar la fórmula en importación o anteponer un apóstrofo de escape (`'`) y forzar el tipo de celda explícito como literal de texto (`DataType.String`) en el archivo `.xlsx` exportado.
3. **Guardas Aritméticas Estrictas**: El motor de validación **DEBE** rechazar cualquier valor numérico `NaN`, `Infinity`, negativo o divisor igual a `0` antes de persistir costes o ejecutar recálculos.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@SEC-REQ-XLSX-001 @automated @regression @security @mitigation
Feature: Protección contra XXE, bombas ZIP e inyección de fórmulas DDE en archivos Excel
  Como componente de importación y generación documental (SEC-ENC-APP-CORE-001)
  Quiero inspeccionar la estructura OOXML y escapar prefijos ejecutables en celdas de texto
  Para proteger al servidor contra agotamiento de memoria/XXE y al cliente contra ejecución de comandos en Excel

  Scenario: Rechazo de plantilla Excel con entidad externa XML (XXE) o ratio de compresión malicioso
    Given un usuario autenticado sube un archivo ".xlsx" al importador de catálogo del centro "C_PROPIO"
    When el parser detecta una declaración DOCTYPE XML externa o un tamaño descomprimido superior a 10 MB
    Then el sistema aborta la lectura inmediatamente y devuelve estado HTTP 422
    And no se persiste ningún registro de coste en la base de datos
    And se registra un evento con severidad "SECURITY" detallando el vector bloqueado

  Scenario Outline: Neutralización de payloads de inyección de fórmulas DDE y valores aritméticos inválidos
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
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial de seguridad para parseo y generación de archivos Excel. |
