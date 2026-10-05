---
id: SEC-REQ-OBS-001
type: security-requirement
title: "RBAC Estricto para Mantenedor, Redactado Automático PII/Secretos en Logs DEBUG y Anti-Log-Injection"
status: draft
version: "1.0.0"
schema-version: "1.0"
category: security
priority: critical
derives-from:
  - UC-OBSERVABILITY-001
mitigates-abuse-case:
  - ABUSE-PRIV-OBS-001
enforced-in-enclave: "SEC-ENC-DATA-OBS-001"
owasp-asvs: "V4.1, V7.1, V7.2"
stride-category: "Elevation of Privilege"
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@SEC-REQ-OBS-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Security Requirement: RBAC Estricto para Mantenedor, Redactado Automático PII/Secretos en Logs DEBUG y Anti-Log-Injection

## 1. Normative Statement

Para mitigar `ABUSE-PRIV-OBS-001` (OWASP ASVS V4.1, V7.1, V7.2) sobre la consola y tubería de observabilidad (`UC-OBSERVABILITY-001`), el sistema **DEBE** garantizar:
1. **Inmutabilidad de Rol en API Pública y RBAC de Mantenedor**: Los endpoints públicos de registro y perfil de usuario **DEBEN** ignorar o rechazar cualquier atributo `role` enviado por el cliente (prevención de *Mass Assignment*). La asignación del rol `ACT-MAINTAINER-001` **SOLO** podrá realizarse mediante provisión administrativa interna y su acceso exigirá verificación explícita de rol en cada petición.
2. **Filtro de Redactado Inmutable Incluso en Nivel `DEBUG`**: Cuando el mantenedor cambie en caliente la granularidad a `DEBUG`, el serializador de logs y trazas **DEBE** aplicar un filtro de ofuscación inmutable (`[REDACTED]`) sobre cualquier campo sensible (`authorization`, `cookie`, `token`, `credential_hash`, `stripe_signature`, `card_number`, `iban`) antes de escribir en disco o exportar en `JSON`/`CSV`/`OTLP`.
3. **Saneamiento Anti-Log-Injection e Integridad de Auditoría**: Todas las entradas de log **DEBEN** codificarse como objetos JSON estructurados de una sola línea, escapando caracteres de control (`\r`, `\n`, `\t`), y todo cambio en caliente del nivel de log **DEBE** generar un registro de auditoría inmutable con severidad `SECURITY` que nunca pueda ser deshabilitado.

## 2. Acceptance Criteria (Gherkin)

```gherkin
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
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial de seguridad sobre RBAC de mantenedor y tubería de logs. |
