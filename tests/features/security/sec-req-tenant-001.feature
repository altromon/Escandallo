# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/security/requirements/sec-req-tenant-001.md
# ID Requerimiento: SEC-REQ-TENANT-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@SEC-REQ-TENANT-001 @automated @regression @security @mitigation
Feature: Prevención de ataques IDOR/BOLA cross-tenant en catálogos e informes
  Como motor de seguridad del núcleo de aplicación (SEC-ENC-APP-CORE-001)
  Quiero validar criptográfica y relacionalmente la propiedad de cada centro de estética
  Para impedir la exfiltración o copia no autorizada de costes y márgenes entre competidores

  Scenario: Rechazo de copia de catálogo M4 desde un centro de estética ajeno
    Given el usuario autenticado "U_ATACANTE" es propietario únicamente del centro "C_PROPIO"
    And existe un centro competidor "C_AJENO" propiedad de otro usuario con 25 cosméticos registrados
    When "U_ATACANTE" solicita clonar el catálogo de productos indicando origen "C_AJENO" y destino "C_PROPIO"
    Then el sistema rechaza la operación con estado HTTP 403
    And el catálogo del centro "C_PROPIO" permanece con 0 productos copiados
    And se registra un evento de auditoría con severidad "SECURITY" y motivo "CROSS_TENANT_IDOR_ATTEMPT"

  Scenario Outline: Bloqueo de acceso IDOR sobre endpoints de costes, escandallos e informes
    Given el usuario autenticado "U_ATACANTE" tiene sesión activa en el centro "C_PROPIO"
    When "U_ATACANTE" invoca la operación "<operacion>" apuntando al recurso "<recurso_ajeno>" del centro "C_AJENO"
    Then el sistema deniega la petición con código HTTP <codigo_http>
    And la respuesta no contiene ningún dato financiero ni nombre comercial de "C_AJENO"
    And se emite una traza de seguridad con nivel "<nivel_log>"

    Examples:
      | operacion                        | recurso_ajeno            | codigo_http | nivel_log |
      | GET /costs                       | center_id=C_AJENO        | 403         | SECURITY  |
      | PATCH /costs/cost-uuid-ajeno     | cost_id=cost-uuid-ajeno  | 404         | SECURITY  |
      | POST /reports/export-pdf         | center_id=C_AJENO        | 403         | SECURITY  |
      | POST /reports/export-xlsx        | center_id=C_AJENO        | 403         | SECURITY  |
