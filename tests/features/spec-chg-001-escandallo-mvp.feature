# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/changes/active/chg-001-escandallo-mvp/spec.md
# ID Requerimiento: SPEC-CHG-001-ESCANDALLO-MVP
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@CHG-001-ESCANDALLO-MVP @functional @automated @regression
Feature: MVP Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética
  As a beauty center owner or system maintainer
  I want to manage multi-tenant cost catalogs, calculate service profitability, export executive reports, and enforce subscription quotas
  So that all functional, quality, and security acceptance criteria for CHG-001-ESCANDALLO-MVP are satisfied

  Scenario: Flujo nominal de extremo a extremo desde alta de costes hasta informe ejecutivo para Dirección y Punto 1 al 100% del ancho de pantalla
    Given el propietario autenticado del centro "Centro Sol" inicializa su catálogo de costes con plantilla semilla en .NET 10 LTS
    When crea su primer escandallo "Higiene Facial" con margen objetivo 0.50 e IVA 0.21 y solicita el informe ejecutivo en PDF
    Then el servidor calcula el Escandallo Total en System.Decimal, evalúa el Semáforo en "OPTIMO" y genera el PDF con código HTTP 200 incluyendo "1. RESUMEN EJECUTIVO" con la información colapsada y "2. DESGLOSE DETALLADO" con la ficha desglosada de cada servicio activo para Dirección
    And la interfaz ordena la página principal con "1. Calculadora de servicios y semáforo de rentabilidad", "2. Catálogo de costes del centro", "3. Impuestos, margen objetivo y equivalencias de dosis de cabina" y "4. Histórico de servicios archivados y actualizaciones de precios"
    And el Punto 1 aprovecha el 100% del ancho de la pantalla y muestra solo los servicios activos colapsados por defecto en una sola línea ocultando sus productos hasta su despliegue, mientras que los servicios archivados pasan al Punto 4 ordenados por fecha de archivado descendente

  Scenario Outline: Análisis de frontera en cuota gratuita de escandallos activos (N-1, N, N+1) y formatos de informe
    Given un centro en plan gratuito posee <escandallos_activos> escandallos activos
    When solicita la operación "<operacion>" con parámetro "<parametro>"
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | escandallos_activos | operacion          | parametro | codigo_http |
      | 1                   | crear_escandallo   | nuevo     | 201         |
      | 2                   | exportar_informe   | pdf       | 200         |
      | 2                   | crear_escandallo   | tercero   | 402         |

@CHG-001-ESCANDALLO-MVP @security @mitigation @automated @regression
Feature: Security Mitigation for CHG-001-ESCANDALLO-MVP
  Scenario Outline: Bloqueo de vectores de ataque cross-tenant, webhooks falsificados, bombas ZIP y abuso DoS
    Given un actor envía una petición con el vector de abuso "<vector_abuso>" hacia el enclave protegido
    When el servidor .NET 10 LTS evalúa los controles de seguridad Zero-Trust y RLS
    Then la petición es rechazada con código HTTP <codigo_http> y se emite un evento de severidad "SECURITY"

    Examples:
      | vector_abuso                         | codigo_http |
      | idor_cross_tenant_center_ajeno       | 403         |
      | webhook_stripe_firma_hmac_invalida   | 400         |
      | xlsx_zipbomb_superior_10mb_real      | 422         |
      | rafaga_circuitos_wss_blazor_exceso   | 429         |
