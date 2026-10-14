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
    Given una petición entrante con estado de autenticación "<estado_auth>" e identificador de centro "<center_id>"
    When el servidor valida el contexto de inquilino antes de acceder al catálogo
    Then el servidor rechaza la solicitud con código HTTP <codigo_http>

    Examples:
      | estado_auth | center_id                 | codigo_http |
      | autenticado |                           | 400         |
      | autenticado | not-a-valid-uuid-format   | 400         |
      | anonimo     | Centro Madrid             | 401         |
