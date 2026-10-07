# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-tenant-001.md
# ID Requerimiento: FR-TENANT-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-TENANT-001 @automated @regression
Feature: Aislamiento Multi-Tenant de Centros de Estética por Cliente

  Scenario: Usuario con múltiples centros copia su catálogo de productos entre sus propios centros manteniendo el aislamiento frente a terceros
    Given el usuario "cliente-a@estetica.es" es propietario de "Centro Madrid" y "Centro Valencia"
    And el usuario "cliente-b@estetica.es" es propietario de "Centro Sevilla"
    When "cliente-a@estetica.es" solicita copiar el catálogo de productos desde "Centro Madrid" hacia "Centro Valencia"
    Then el servidor copia los productos hacia "Centro Valencia" con código HTTP 200
    And los datos de "Centro Sevilla" permanecen inaccesibles para "cliente-a@estetica.es"

  Scenario Outline: Denegación de acceso o copia cruzada entre centros de distintos clientes
    Given el usuario "<usuario_autenticado>" está autenticado en el servidor
    And el centro "<centro_objetivo>" pertenece al propietario "<propietario_real>"
    When "<usuario_autenticado>" intenta ejecutar la acción "<accion>" sobre "<centro_objetivo>"
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | usuario_autenticado   | centro_objetivo | propietario_real      | accion                     | codigo_http |
      | cliente-a@estetica.es | Centro Madrid   | cliente-a@estetica.es | consultar_catalogo_costes  | 200         |
      | cliente-a@estetica.es | Centro Valencia | cliente-a@estetica.es | copiar_catalogo_propio     | 200         |
      | cliente-a@estetica.es | Centro Sevilla  | cliente-b@estetica.es | consultar_catalogo_costes  | 403         |
      | cliente-b@estetica.es | Centro Madrid   | cliente-a@estetica.es | copiar_catalogo_ajeno      | 403         |

  @security @mitigation
  Scenario Outline: Rechazo controlado de identificadores de centro fuera de rango, vacíos o no autorizados
    Given una petición hacia el endpoint multi-tenant con sesión "<estado_sesion>"
    When se envía el identificador de centro "<center_id_invalido>" para la operación "<operacion>"
    Then el servidor rechaza la operación de forma controlada con código HTTP <codigo_error> sin exponer trazas internas

    Examples:
      | estado_sesion | center_id_invalido                   | operacion          | codigo_error |
      | autenticado   | ""                                   | listar_escandallos | 400          |
      | autenticado   | "not-a-valid-uuid-format"            | copiar_catalogo    | 400          |
      | anonimo       | "550e8400-e29b-41d4-a716-4466554400" | listar_escandallos | 401          |
