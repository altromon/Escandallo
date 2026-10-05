# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/changes/active/chg-001-escandallo-mvp/spec.md
# ID Requerimiento: SPEC-CHG-001-ESCANDALLO-MVP
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@CHG-001-ESCANDALLO-MVP @functional @automated
Feature: Entrega integral del MVP de la Plataforma SaaS Multi-Tenant de Escandallos para Centros de Estética
  Como propietario de un centro de estética o usuario mantenedor
  Quiero gestionar catálogos de costes, calcular escandallos con semáforo de margen, exportar informes PDF/Excel, gestionar suscripciones Stripe y auditar telemetría
  Para decidir el PVP rentable de los servicios estéticos con aislamiento multi-tenant garantizado

  Scenario: Flujo extremo a extremo de alta de centro, cálculo de escandallo con merma, recálculo en cascada y exportación PDF/Excel
    Given un cliente autenticado inicializa su centro con la plantilla semilla de "CalculadoraEscandallo.xlsx"
    When calcula el escandallo del servicio "Higiene Facial" de 60 minutos y actualiza posteriormente el coste de un producto en uso
    Then el sistema recalcula automáticamente el escandallo, genera el snapshot histórico inmutable y permite exportar el informe ejecutivo en PDF y Excel

@CHG-001-ESCANDALLO-MVP @security @mitigation
Feature: Blindaje integral Zero-Trust frente a IDOR, Race Conditions de Cuota, XXE/DDE, DoS y Escalada a Mantenedor
  Scenario: Mitigación coordinada de vectores de ataque multi-tenant y de integridad financiera
    Given un actor malicioso intenta acceder a otro centro, evadir la cuota de 2 escandallos gratuitos, inyectar fórmulas Excel o escalar al rol mantenedor
    When la petición alcanza los enclaves protegidos SEC-ENC-EDGE-WAF-001, SEC-ENC-APP-CORE-001 y SEC-ENC-DATA-OBS-001
    Then la operación es rechazada inmediatamente y se registra un evento inmutable con severidad "SECURITY" y secretos redactados con "[REDACTED]"
