---
id: SEC-REQ-OBS-001
type: security-requirement
title: "RBAC Estricto para Mantenedor, Redactado Automático PII/Secretos en Logs DEBUG y Anti-Log-Injection"
status: draft
version: "1.1.0"
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

Para mitigar `ABUSE-PRIV-OBS-001` (OWASP ASVS V4.1, V7.1, V7.2) sobre `MaintainerObservabilityPage.razor` (`CMP-WEB-UI-001`) y el subsistema `Serilog` + `OpenTelemetry .NET` (`CMP-OBS-TELEMETRY-001` en `SEC-ENC-DATA-OBS-001`), el sistema **DEBE** garantizar:
1. **Inmutabilidad de Rol en ASP.NET Core Identity y Autorización en Blazor**: Los endpoints y formularios públicos de registro/perfil **DEBEN** ignorar o rechazar cualquier atributo `role` enviado por el cliente (*Mass Assignment*), y `MaintainerObservabilityPage.razor` / `IObservabilityControlService` **DEBEN** exigir `[Authorize(Roles = "Maintainer")]` verificando el `ClaimsPrincipal` en servidor en cada invocación.
2. **Filtro `PiiAndSecretRedactingPolicy` Inmutable y Cota de Destructuración en `Serilog`**: Cuando `LoggingLevelSwitch` se conmute en caliente a `DEBUG`, `Serilog` **DEBE** limitar la destructuración (`Destructuring.ToMaximumDepth(3)`, `ToMaximumStringLength(1024)`) y aplicar `PiiAndSecretRedactingPolicy` (`[REDACTED]`) tanto sobre propiedades estructuradas (`authorization`, `cookie`, `token`, `credential_hash`, `stripe_signature`, `card_number`, `iban`) como sobre cadenas de conexión `Npgsql` (`Password=...`) y claves `whsec_` dentro de `Exception.Message` antes de exportar a `JSON`/`CSV`/`OTLP`.
3. **Saneamiento Anti-Log-Injection y Canal Inmutable para Eventos `SECURITY`**: Todas las entradas de log **DEBEN** serializarse como JSON de una sola línea escapando `\r`, `\n` y `\t`, y los eventos de severidad `SECURITY` (incluido cada cambio de `LoggingLevelSwitch`) **DEBEN** persistirse en un canal/almacén protegido contra evicción por rotación de logs `DEBUG`/`INFO`.

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

  Scenario Outline: Bloqueo de escalada de privilegios a mantenedor, fugas en excepciones e inyección de logs
    Given un atacante externo o inquilino estándar envía una petición con el vector "<vector_ataque>"
    When el subsistema de seguridad y telemetría procesa la solicitud con payload "<payload_malicioso>"
    Then el sistema ejecuta la respuesta defensiva "<respuesta_defensiva>" con código HTTP <codigo_http>
    And registra un evento íntegro de una sola línea JSON con severidad "SECURITY"

    Examples:
      | vector_ataque                        | payload_malicioso                        | respuesta_defensiva                       | codigo_http |
      | mass_assignment_registro_rol         | role=maintainer                          | ignorar_campo_y_denegar_privilegio        | 403         |
      | acceso_endpoint_exportacion_logs     | GET /admin/observability/export          | denegar_acceso_rbac_no_autorizado         | 403         |
      | inyeccion_crlf_en_campo_busqueda     | servicio\r\n{"level":"INFO","faked":1}   | escapar_saltos_de_linea_en_json_estruct   | 400         |
      | excepcion_npgsql_con_password_debug  | Host=db;Password=Secret123;              | redactar_credencial_en_exception_message  | 500         |

  Scenario Outline: Verificación de frontera de ofuscación inmutable en todos los niveles de severidad, profundidad de destructuración (Depth 3) y caducidad TTL de DEBUG
    Given el subsistema de telemetría opera en nivel "<nivel_log>" con profundidad de objeto <profundidad_obj> tras <minutos_transcurridos> minutos desde la activación de DEBUG (TTL 60 min)
    When procesa un evento con el campo sensible "<campo_sensible>"
    Then el valor registrado es "<valor_registrado>" y el nivel efectivo del sistema es "<nivel_efectivo>"

    Examples:
      | nivel_log | profundidad_obj | minutos_transcurridos | campo_sensible   | valor_registrado | nivel_efectivo |
      | DEBUG     | 2               | 59                    | authorization    | [REDACTED]       | DEBUG          |
      | DEBUG     | 3               | 60                    | stripe_signature | [REDACTED]       | INFO           |
      | SECURITY  | 4               | 61                    | credential_hash  | [REDACTED]       | INFO           |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial de seguridad sobre RBAC de mantenedor y tubería de logs. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Auditoría técnica .NET 10 LTS + Serilog: límite `ToMaximumDepth(3)`, redacción en `Exception.Message` y protección anti-evicción de eventos `SECURITY`. |
