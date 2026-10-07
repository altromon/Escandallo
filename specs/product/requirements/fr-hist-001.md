---
id: FR-HIST-001
type: requirement
title: Registro Histórico y Recálculo Automático en Cascada de Escandallos
status: active
version: 1.0.0
schema-version: '1.0'
category: functional
derives-from:
  - UC-ESCANDALLO-CALC-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - '@FR-HIST-001'
  - '@automated'
  - '@regression'
supersedes: null
superseded-by: null
---


# Requirement: Registro Histórico y Recálculo Automático en Cascada de Escandallos

## 1. Normative Statement

El sistema DEBE almacenar un registro histórico inmutable de las versiones de un coste (producto, personal, gasto general o parámetro global) **únicamente si existía al menos un escandallo utilizándolo en el momento del cambio**, mostrando una previsualización del número de escandallos afectados antes de confirmar (Mejora M3), recalculando automáticamente todos los escandallos vinculados, generando una nueva instantánea en el histórico de cada servicio recalculado e impidiendo el borrado físico de costes con histórico mediante baja lógica (`Archivado`, Mejora M1).

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-HIST-001 @automated @regression
Feature: Registro Histórico y Recálculo Automático en Cascada de Escandallos

  Scenario: Previsualización de impacto, recálculo automático y baja lógica de un producto en uso
    Given el servicio "Peeling Facial" utiliza 5 ml del producto "Lifting Peeling" cuyo envase de 100 ml cuesta 100.00 EUR (1.00 EUR/ml)
    And el Escandallo Total actual de "Peeling Facial" es 20.00 EUR en la versión histórica 1
    When el usuario actualiza el precio del envase de "Lifting Peeling" de 100.00 EUR a 120.00 EUR (1.20 EUR/ml)
    Then el sistema informa que el cambio recalculará 1 escandallo y conserva la versión anterior de 100.00 EUR en el histórico de costes
    And el sistema recalcula automáticamente "Peeling Facial" actualizando su Escandallo Total a 21.00 EUR en la versión histórica 2
    And si el usuario intenta eliminar "Lifting Peeling", el sistema impide el borrado físico y lo marca como "archived"

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
    Given un coste tiene <versiones_previas> versiones en el histórico y <escandallos_activos> escandallos vinculados
    When un usuario intenta ejecutar la operación restringida "<operacion_invalida>"
    Then el servidor rechaza o reconduce la operación con estado "<estado_respuesta>" y código HTTP <codigo_error>

    Examples:
      | versiones_previas | escandallos_activos | operacion_invalida                 | estado_respuesta          | codigo_error |
      | 2                 | 1                   | DELETE_FISICO_COSTE_EN_USO         | FORZAR_ARCHIVADO_LOGICO   | 409          |
      | 3                 | 0                   | DELETE_SNAPSHOT_HISTORICO          | HISTORICO_INMUTABLE       | 403          |
      | 1                 | 2                   | UPDATE_COSTE_SIN_CAMBIO_DE_IMPORTE | IDEMPOTENTE_SIN_SNAPSHOT  | 200          |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de histórico y recálculo automático en cascada. |
| 1.1.0 | 2026-10-05 | agent-expert-user | Condición de guardar histórico de costes solo si algún escandallo utiliza el coste en el momento del cambio. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de mejoras M1 (baja lógica de costes con histórico) y M3 (previsualización de impacto). |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-07 | agent-devops | Implementación verificada (TSK-001, TSK-002, TSK-003, TSK-004, TSK-005, TSK-006, TSK-007) | chg-001-escandallo-mvp |
