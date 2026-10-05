# ==============================================================================
# AUTO-GENERADO POR AI-SDLC (Cucumber Integration)
# Origen: specs/security/requirements/sec-req-obs-001.md
# ID Requerimiento: SEC-REQ-OBS-001
# Versión: 1.0.0
# NO EDITAR MANUALMENTE: Cualquier cambio debe realizarse en el Markdown origen.
# ==============================================================================

@SEC-REQ-OBS-001 @automated @regression @security @mitigation
Feature: Blindaje del rol mantenedor, ofuscación de secretos en modo DEBUG y prevención de Log Forgery
  Como subsistema de telemetría y auditoría (SEC-ENC-DATA-OBS-001)
  Quiero restringir el acceso administrativo y redactar automáticamente credenciales en todos los niveles de log
  Para evitar la escalada de privilegios y la fuga de secretos en exportaciones de observabilidad

  Scenario: Ofuscación obligatoria de cabeceras de sesión y firmas de pago cuando el log está en nivel DEBUG
    Given un usuario con rol "ACT-MAINTAINER-001" ha configurado en caliente el nivel de log en "DEBUG"
    When un usuario de centro de estética inicia sesión y procesa una petición autenticada con cabeceras sensibles
    And el mantenedor exporta los registros de telemetría en formato "JSON"
    Then los campos de autorización, cookies y firmas aparecen reemplazados por el marcador "[REDACTED]"
    And ningún token de sesión en claro está presente en el archivo exportado

  Scenario Outline: Bloqueo de escalada de privilegios a mantenedor e intentos de inyección de logs
    Given un atacante externo o inquilino estándar envía una petición con el vector "<vector_ataque>"
    When el subsistema de seguridad y telemetría procesa la solicitud con payload "<payload_malicioso>"
    Then el sistema ejecuta la respuesta defensiva "<respuesta_defensiva>" con código HTTP <codigo_http>
    And registra un evento íntegro de una sola línea JSON con severidad "SECURITY"

    Examples:
      | vector_ataque                        | payload_malicioso                        | respuesta_defensiva                       | codigo_http |
      | mass_assignment_registro_rol         | role=maintainer                          | ignorar_campo_y_denegar_privilegio        | 403         |
      | acceso_endpoint_exportacion_logs     | GET /admin/observability/export          | denegar_acceso_rbac_no_autorizado         | 403         |
      | inyeccion_crlf_en_campo_busqueda     | servicio\r\n{"level":"INFO","faked":1}   | escapar_saltos_de_linea_en_json_estruct   | 400         |
