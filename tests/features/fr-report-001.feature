# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/product/requirements/fr-report-001.md
# ID Requerimiento: FR-REPORT-001
# Versión: 1.3.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@FR-REPORT-001 @automated @regression
Feature: Generación y Exportación de Informes Ejecutivos de Costes y Beneficios

  Scenario: Extracción de informe ejecutivo en PDF para Dirección con Resumen Ejecutivo inicial colapsado y listado de cada servicio desglosado
    Given el centro "Centro Estética Norte" tiene 2 servicios activos calculados dentro del límite gratuito
    When el propietario solicita extraer el informe ejecutivo del centro en formato "pdf"
    Then el servidor genera el documento PDF maquetado para Dirección incluyendo al principio la sección "1. RESUMEN EJECUTIVO" con la tabla consolidada de servicios activos y sus datos de vista colapsada (Servicio, Duración en min, Personal, Coste Total, Precio de Venta, Precio Recomendado y dictamen de rentabilidad en lenguaje natural)
    And a continuación incluye la sección "2. DESGLOSE DETALLADO" con la ficha individual de cada servicio activo detallando Productos con merma, Personal imputado, Gastos Generales, Escandallo Total, Beneficio Neto Objetivo, Precio sin IVA, PVP Recomendado, PVP Comercial Fijado, Beneficio Neto Real, Margen Neto Real y la tabla de productos cosméticos de la receta
    And el documento PDF excluye cualquier servicio archivado y no expone literales crudos de enumerados internos

  Scenario Outline: Validación de formatos soportados (PDF y Excel) en el informe ejecutivo
    Given el centro tiene un servicio "<servicio>" con Gastos Generales <gastos_gen> EUR, Coste Persona <coste_pers> EUR y Productos <coste_prod> EUR
    When el usuario solicita exportar el informe ejecutivo en formato "<formato>"
    Then el servidor responde con código HTTP <codigo_http> para el formato "<formato>"

    Examples:
      | servicio           | gastos_gen | coste_pers | coste_prod | formato | codigo_http |
      | Tratamiento Facial | 5.00       | 11.25      | 3.75       | pdf     | 200         |
      | Dermapen Completo  | 5.00       | 18.00      | 17.00      | xlsx    | 200         |
      | Tratamiento Facial | 5.00       | 11.25      | 3.75       | csv     | 400         |

  @performance
  Scenario Outline: Análisis de frontera del número de escandallos activos (N-1, N, N+1) al exportar informes en plan gratuito
    Given un centro de estética en plan gratuito ("none") posee exactamente <escandallos_activos> escandallos activos
    When el propietario solicita exportar el informe ejecutivo en formato "<formato>"
    Then el servidor responde con código HTTP <codigo_http> dentro del umbral de rendimiento

    Examples:
      | escandallos_activos | formato | codigo_http |
      | 1                   | pdf     | 200         |
      | 2                   | xlsx    | 200         |
      | 3                   | pdf     | 402         |
