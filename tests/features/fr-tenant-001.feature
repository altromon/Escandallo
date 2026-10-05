# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-tenant-001.md
# ID Requerimiento: FR-TENANT-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-TENANT-001 @automated @regression @security
Feature: Aislamiento Multi-Tenant de Centros de Estética por Cliente

  Scenario: Usuario con múltiples centros copia su catálogo de productos entre sus propios centros manteniendo el aislamiento frente a terceros
    Given el usuario "cliente-a@estetica.es" es propietario de "Centro Madrid" y "Centro Valencia"
    And el usuario "cliente-b@estetica.es" es propietario de "Centro Sevilla"
    When "cliente-a@estetica.es" solicita copiar el catálogo de productos desde "Centro Madrid" hacia "Centro Valencia"
    Then el servidor copia los productos hacia "Centro Valencia" con código HTTP 200
    And los datos de "Centro Sevilla" permanecen inaccesibles para "cliente-a@estetica.es"

  @boundary @idempotence
  Scenario Outline: Casos límite de titularidad y volumen al clonar o consultar catálogos entre centros del mismo propietario
    Given el usuario "<usuario_autenticado>" posee <num_centros> centros registrados y el centro origen tiene <items_catalogo> productos
    And el centro "<centro_objetivo>" pertenece al propietario "<propietario_real>"
    When "<usuario_autenticado>" ejecuta la acción "<accion>" sobre "<centro_objetivo>"
    Then el servidor responde con código HTTP <codigo_http> y procesa <items_copiados> productos

    Examples:
      | usuario_autenticado   | num_centros | items_catalogo | centro_objetivo | propietario_real      | accion                    | codigo_http | items_copiados |
      | cliente-a@estetica.es | 1           | 0              | Centro Madrid   | cliente-a@estetica.es | consultar_catalogo_vacio  | 200         | 0              |
      | cliente-a@estetica.es | 2           | 1              | Centro Valencia | cliente-a@estetica.es | copiar_catalogo_minimo    | 200         | 1              |
      | cliente-a@estetica.es | 2           | 5000           | Centro Valencia | cliente-a@estetica.es | copiar_catalogo_max_filas | 200         | 5000           |

  @invalid @security
  Scenario Outline: Rechazo de accesos fuera de rango, identificadores malformados o copia cruzada entre distintos clientes
    Given el usuario "<usuario_autenticado>" está autenticado en el servidor
    And el centro objetivo tiene identificador "<centro_id>" perteneciente a "<propietario_real>"
    When "<usuario_autenticado>" intenta ejecutar la acción "<accion>" sobre "<centro_id>"
    Then el servidor rechaza la petición con código HTTP <codigo_http>

    Examples:
      | usuario_autenticado   | centro_id                            | propietario_real      | accion                    | codigo_http |
      | cliente-a@estetica.es | 00000000-0000-4000-8000-000000000099 | cliente-b@estetica.es | consultar_catalogo_costes | 403         |
      | cliente-b@estetica.es | 00000000-0000-4000-8000-000000000001 | cliente-a@estetica.es | copiar_catalogo_ajeno     | 403         |
      | cliente-a@estetica.es | id-no-uuid-invalido                  | ninguno               | consultar_catalogo_costes | 400         |
      | cliente-a@estetica.es | ""                                   | ninguno               | copiar_catalogo_propio    | 400         |
