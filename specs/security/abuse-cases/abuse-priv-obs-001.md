---
id: ABUSE-PRIV-OBS-001
type: abuse-case
title: "Escalada de Privilegios al Rol Mantenedor, Log Forgery y Fuga de Secretos en Modo DEBUG"
status: draft
version: "1.0.0"
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

El rol `ACT-MAINTAINER-001` dispone de capacidades privilegiadas para consultar y exportar logs, trazas y métricas de toda la plataforma, así como cambiar en caliente el nivel de severidad a `DEBUG` (`UC-OBSERVABILITY-001`). El atacante (`ACT-THREAT-EXT-001`) intenta escalar privilegios a este rol o aprovechar la telemetría para extraer credenciales y falsificar registros de auditoría.

## 2. Attack Flow (Step-by-Step)

1. **Vector A (Privilege Escalation / Role Tampering)**: El atacante intenta registrarse o actualizar su perfil inyectando `"role": "maintainer"` (Mass Assignment) o manipulando los claims del token JWT para invocar los endpoints de `/admin/observability`.
2. **Vector B (Exfiltración vía Nivel `DEBUG`)**: Cuando el mantenedor activa temporalmente el nivel `DEBUG` en caliente, si el logger no aplica redactado automático (*redaction*), las trazas vuelcan cabeceras `Authorization`, cookies de sesión, firmas de Stripe o datos personales financieros en los archivos exportables (`JSON`/`CSV`/`OTLP`).
3. **Vector C (Log Injection / Forgery)**: El atacante envía saltos de línea (`\r\n`) o payloads JSON malformados en campos de entrada para inyectar entradas falsas en los logs de auditoría y ocultar sus ataques.

## 3. Business & Security Impact

- **Compromiso Total de la Plataforma**: El robo de tokens o claves de API a través de logs `DEBUG` permite secuestrar cuentas de cualquier centro de estética o manipular la pasarela de pagos.

## 4. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial del caso de abuso sobre observabilidad y rol mantenedor. |
