# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-hist-001.md
# ID Requerimiento: FR-HIST-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-HIST-001 @automated @regression @idempotence
Feature: Registro Histórico y Recálculo Automático en Cascada de Escandallos

  Scenario: Previsualización de impacto, recálculo automático y baja lógica de un producto en uso
    Given el servicio "Peeling Facial" utiliza 5 ml del producto "Lifting Peeling" cuyo envase de 100 ml cuesta 100.00 EUR (1.00 EUR/ml)
    And el Escandallo Total actual de "Peeling Facial" es 20.00 EUR en la versión histórica 1
    When el usuario actualiza el precio del envase de "Lifting Peeling" de 100.00 EUR a 120.00 EUR (1.20 EUR/ml)
    Then el sistema informa que el cambio recalculará 1 escandallo y conserva la versión anterior de 100.00 EUR en el histórico de costes
    And el sistema recalcula automáticamente "Peeling Facial" actualizando su Escandallo Total a 21.00 EUR en la versión histórica 2
    And si el usuario intenta eliminar "Lifting Peeling", el sistema impide el borrado físico y lo marca como "archived"

  @boundary
  Scenario Outline: Comportamiento límite del histórico según el número de escandallos vinculados al coste modificado
    Given un coste de tipo "<tipo_coste>" tiene <escandallos_vinculados> escandallos que lo utilizan actualmente
    When el usuario modifica el importe de dicho coste en el catálogo del centro
    Then el número de nuevas entradas generadas en el histórico de costes es <entradas_historico_coste>
    And el número de escandallos recalculados automáticamente es <escandallos_recalculados>

    Examples:
      | tipo_coste    | escandallos_vinculados | entradas_historico_coste | escandallos_recalculados |
      | producto      | 0                      | 0                        | 0                        |
      | personal      | 0                      | 0                        | 0                        |
      | producto      | 1                      | 1                        | 1                        |
      | gasto_general | 2                      | 1                        | 2                        |
      | parametro_iva | 50                     | 1                        | 50                       |

  @invalid
  Scenario Outline: Rechazo de borrado físico forzoso, mutación sin cambio de valor o modificación de registros históricos inmutables
    Given un coste de tipo "<tipo_coste>" con estado "<estado_coste>" y <versiones_previas> versiones en el histórico
    When el usuario intenta ejecutar la operación inválida "<operacion_invalida>"
    Then el servidor rechaza la operación con código HTTP <codigo_http> y código de error "<error_code>"

    Examples:
      | tipo_coste    | estado_coste | versiones_previas | operacion_invalida                   | codigo_http | error_code                     |
      | producto      | active       | 1                 | borrado_fisico_con_historico         | 409         | PHYSICAL_DELETE_FORBIDDEN      |
      | personal      | archived     | 2                 | borrado_fisico_coste_archivado       | 409         | PHYSICAL_DELETE_FORBIDDEN      |
      | gasto_general | active       | 3                 | sobrescribir_snapshot_historico_v1   | 403         | IMMUTABLE_HISTORY_VIOLATION    |
      | producto      | active       | 1                 | actualizar_con_importe_negativo      | 422         | INVALID_COST_UPDATE_AMOUNT     |
