# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-hist-001.md
# ID Requerimiento: FR-HIST-001
# Versión: 1.3.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-HIST-001 @automated @regression
Feature: Registro Histórico y Recálculo Automático en Cascada de Escandallos

  Scenario: Previsualización de impacto, recálculo automático, baja lógica y ordenación de servicios archivados por fecha descendente en el histórico
    Given el servicio "Peeling Facial" utiliza 5 ml del producto "Lifting Peeling" cuyo envase de 100 ml cuesta 100.00 EUR (1.00 EUR/ml)
    And el Escandallo Total actual de "Peeling Facial" es 20.00 EUR en la versión histórica 1
    When el usuario actualiza el precio del envase de "Lifting Peeling" de 100.00 EUR a 120.00 EUR (1.20 EUR/ml)
    Then el sistema informa que el cambio recalculará 1 escandallo y conserva la versión anterior de 100.00 EUR en el histórico de costes
    And el sistema recalcula automáticamente "Peeling Facial" actualizando su Escandallo Total a 21.00 EUR en la versión histórica 2
    And si el usuario intenta eliminar "Lifting Peeling", el sistema impide el borrado físico y lo marca como "archived"
    And cuando el usuario archiva servicios activos, se retiran de la calculadora principal y se listan en la sección de histórico ordenados por fecha de archivado descendente

  Scenario Outline: Comportamiento del histórico según si el coste modificado está en uso por algún escandallo
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

  @idempotence @performance
  Scenario Outline: Rechazo de borrado físico de costes con histórico o mutación inválida del registro inmutable
    Given un coste con identificador "<coste_id>" posee <versiones_previas> versiones en el histórico y está vinculado a <escandallos_activos> escandallos
    When un cliente solicita ejecutar la acción "<accion_invalida>" sobre dicho coste o su histórico
    Then el servidor responde con código HTTP <codigo_http> y estado "<resultado_integridad>"

    Examples:
      | coste_id          | versiones_previas | escandallos_activos | accion_invalida                    | codigo_http | resultado_integridad               |
      | prod-en-uso-01    | 2                 | 3                   | DELETE_FISICO_COSTE_EN_USO         | 409         | BLOQUEADO_SOLO_ARCHIVADO_PERMITIDO |
      | hist-snap-0001    | 1                 | 1                   | DELETE_SNAPSHOT_HISTORICO          | 403         | HISTORICO_INMUTABLE_PROTEGIDO      |
      | prod-en-uso-01    | 2                 | 3                   | UPDATE_COSTE_SIN_CAMBIO_DE_IMPORTE | 200         | IDEMPOTENTE_SIN_NUEVA_VERSION      |
