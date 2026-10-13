---
id: ABUSE-PRIV-OBS-001
type: abuse-case
title: "Escalada de Privilegios al Rol Mantenedor, Log Forgery y Fuga de Secretos en Modo DEBUG"
status: draft
version: "1.1.0"
schema-version: "1.0"
primary-threat-actor: ACT-THREAT-EXT-001
targets-use-cases:
  - UC-OBSERVABILITY-001
stride-category: "Elevation of Privilege"
severity: "CRITICAL"
likelihood: "MEDIUM"
mitigated-by:
  - SEC-REQ-OBS-001
supersedes: null
superseded-by: null
---

# Abuse Case: Escalada de Privilegios al Rol Mantenedor, Log Forgery y Fuga de Secretos en Modo DEBUG

## 1. Attack Scenario & Preconditions

El rol `ACT-MAINTAINER-001` dispone de la vista `MaintainerObservabilityPage.razor` (`CMP-WEB-UI-001`) y del servicio `IObservabilityControlService` (`CMP-OBS-TELEMETRY-001` en `SEC-ENC-DATA-OBS-001`) para conmutar en caliente `Serilog.Core.LoggingLevelSwitch` a `DEBUG` y exportar telemetría (`JSON`, `CSV`, `OTLP`). El atacante (`ACT-THREAT-EXT-001`) intenta escalar privilegios o explotar el pipeline `Serilog` + `OpenTelemetry .NET`.

## 2. Attack Flow (Step-by-Step)

1. **Vector A (Privilege Escalation / Overposting en ASP.NET Core Identity & Blazor)**: El atacante intenta registrarse, actualizar su perfil o invocar directamente acciones de `MaintainerObservabilityPage.razor` inyectando `"role": "maintainer"` (*Mass Assignment*) o manipulando claims de autenticación.
2. **Vector B (Exfiltración en Modo `DEBUG` y Bombas de Destructuración en `Serilog`)**: Cuando `LoggingLevelSwitch` se activa en `DEBUG`, el atacante fuerza excepciones en `Npgsql` o `Stripe.net` esperando que la destructuración profunda de objetos (`{@Exception}`, `{@HttpContext}`) vuelque cadenas de conexión con `Password=...`, claves `whsec_`, cookies `.AspNetCore.Identity.Application` o provoque agotamiento de CPU por referencias circulares.
3. **Vector C (Log Injection CRLF y Evicción del Ring-Buffer de Seguridad)**: El atacante inyecta secuencias `\r\n` en nombres de catálogo o genera miles de eventos `INFO`/`DEBUG` por minuto con el objetivo de rotar y sobrescribir las alertas `SECURITY` en el almacén circular (`RingBuffer`) antes de que el mantenedor las audite.

## 3. Business & Security Impact

- **Compromiso Total de la Plataforma y Repudio de Auditoría**: El robo de credenciales de base de datos, cookies de identidad o secretos de Stripe en trazas `DEBUG` permite secuestrar el servidor o cualquier centro de estética, mientras que la rotación forzada del *ring-buffer* borra las huellas forenses del ataque.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso sobre observabilidad y rol mantenedor. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Incorporación de vectores técnicos sobre destructuración Serilog, fugas en Exception.Message y evicción del RingBuffer. |
