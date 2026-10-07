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

  Scenario Outline: Análisis de frontera en tamaño comprimido (2MB), descomprimido (10MB), ratio ZIP (20:1) y filas (5000)
    Given un archivo ".xlsx" tiene tamaño comprimido <tam_comprimido_kb> KB, tamaño descomprimido <tam_descomprimido_kb> KB, ratio <ratio_zip> y <num_filas> filas
    When el validador OOXML pre-inspecciona las cabeceras del archivo antes de cargarlo en memoria
    Then el resultado de la inspección es "<veredicto_zip>" con código HTTP <codigo_http>

    Examples:
      | tam_comprimido_kb | tam_descomprimido_kb | ratio_zip | num_filas | veredicto_zip                    | codigo_http |
      | 2047              | 10239                | 19        | 4999      | ARCHIVO_OOXML_VALIDO             | 200         |
      | 2048              | 10240                | 20        | 5000      | ARCHIVO_OOXML_VALIDO             | 200         |
      | 2049              | 5000                 | 5         | 1000      | RECHAZADO_TAMANO_COMPRIMIDO      | 422         |
      | 1000              | 10241                | 21        | 1000      | RECHAZADO_RATIO_O_DESCOMPRIMIDO  | 422         |
      | 500               | 2000                 | 4         | 5001      | RECHAZADO_LIMITE_FILAS_EXCEDIDO  | 422         |
