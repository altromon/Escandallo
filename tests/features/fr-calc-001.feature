# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-calc-001.md
# ID Requerimiento: FR-CALC-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-CALC-001 @automated @regression
Feature: Cálculo de Escandallo Total, Beneficio y PVP de Servicios Estéticos

  Scenario: Cálculo completo del desglose de un servicio estético con productos, personal y gastos generales
    Given un centro de estética tiene una tasa de gastos generales de 0.08285985 EUR/min
    And existe la categoría "Tecnico" con precio unitario 0.1875 EUR/min
    And existen los productos "Cleasing Milk" a 0.06902 EUR/ml y "Colagen Mask" a 7.21 EUR/ud con merma 0%
    And los parámetros del centro son Beneficio 0.50 e IVA 0.21
    When el usuario calcula el escandallo del servicio "Higiene Facial" de 60 minutos con "Tecnico", 10 ml de "Cleasing Milk" y 1 ud de "Colagen Mask"
    Then los Gastos Generales imputados al servicio son 4.971591 EUR
    And el Coste Persona del servicio es 11.25 EUR
    And la suma de Costes de Productos es 7.9002 EUR
    And el Escandallo Total del servicio es 24.121791 EUR
    And el Beneficio Neto unitario es 12.0608955 EUR
    And el Precio con Beneficio sin IVA es 36.1826865 EUR
    And el PVP final con IVA es 43.78105067 EUR

  Scenario Outline: Evaluación de Beneficio Real y Semáforo de Erosión de Margen frente al PVP Comercial Fijado
    Given un servicio tiene un Escandallo Total calculado de <escandallo_total> EUR, un Beneficio objetivo de <beneficio_obj> y un IVA de <iva_pct>
    When el usuario establece un PVP Comercial Fijado de <pvp_comercial> EUR
    Then el Precio sin IVA Real es <precio_sin_iva> EUR
    And el Beneficio Neto Real es <beneficio_neto> EUR
    And el Semáforo de Erosión de Margen muestra el estado "<semaforo>"

    Examples:
      | escandallo_total | beneficio_obj | iva_pct | pvp_comercial | precio_sin_iva | beneficio_neto | semaforo |
      | 20.00            | 0.50          | 0.21    | 36.30         | 30.00          | 10.00          | OPTIMO   |
      | 25.00            | 0.50          | 0.21    | 36.30         | 30.00          | 5.00           | ALERTA   |
      | 32.00            | 0.50          | 0.21    | 36.30         | 30.00          | -2.00          | CRITICO  |

  Scenario Outline: Rechazo de cálculo de escandallo ante parámetros fuera de rango o dosis inválidas
    Given un usuario configura un servicio estético en el constructor dinámico
    When envía una duración de <duracion_min> minutos, dosis de producto <dosis>, margen <margen> e IVA <iva>
    Then el motor de cálculo rechaza la solicitud con código HTTP <codigo_error> y error "<codigo_validacion>"

    Examples:
      | duracion_min | dosis | margen | iva   | codigo_error | codigo_validacion             |
      | 0            | 10.0  | 0.50   | 0.21  | 422          | DURACION_SERVICIO_INVALIDA    |
      | -30          | 10.0  | 0.50   | 0.21  | 422          | DURACION_SERVICIO_INVALIDA    |
      | 60           | -5.0  | 0.50   | 0.21  | 422          | DOSIS_PRODUCTO_NEGATIVA       |
      | 60           | 10.0  | -1.20  | 0.21  | 422          | MARGEN_BENEFICIO_FUERA_RANGO  |
      | 60           | 10.0  | 0.50   | -0.10 | 422          | IVA_NEGATIVO_INVALIDO         |
