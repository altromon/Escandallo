# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/security/requirements/sec-req-xlsx-001.md
# ID Requerimiento: SEC-REQ-XLSX-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

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
