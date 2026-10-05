---
id: ACT-THREAT-EXT-001
type: threat-actor
title: "Atacante Cibernético Externo y Botnet Automatizada"
status: draft
version: "1.0.0"
schema-version: "1.0"
category: "external-attacker"
skill-level: "high"
motivation: "Compromiso del servidor, denegación de servicio (DoS), fraude de pagos falsificando webhooks y exfiltración de telemetría"
stride-categories:
  - "Spoofing"
  - "Tampering"
  - "Repudiation"
  - "Information Disclosure"
  - "Denial of Service"
  - "Elevation of Privilege"
supersedes: null
superseded-by: null
---

# Threat Actor: Atacante Cibernético Externo y Botnet Automatizada

## 1. Adversary Profile & Motivation

Adversario externo en Internet que ataca la superficie pública del servidor web buscando comprometer la infraestructura, agotar recursos computacionales (CPU/RAM/disco), activar suscripciones de forma fraudulenta o escalar privilegios hacia el rol `ACT-MAINTAINER-001`.

## 2. Attack Capabilities & Vectors

- **Spoofing & Replay de Webhooks**: Envío de peticiones HTTP POST falsificadas al endpoint de webhooks de Stripe.
- **Exploits sobre Importador `.xlsx` (Mejora M4)**: Subida de archivos Excel manipulados con entidades externas XML (*XXE*), bombas de compresión (*Zip Bombs*) o inyección de fórmulas maliciosas (*CSV/Excel Formula Injection*).
- **Denegación de Servicio Asimétrica (Application DoS)**: Disparo masivo de recálculos en cascada y exportaciones concurrentes de informes PDF/Excel.
- **Escalada de Privilegios y Log Injection**: Ataques contra los endpoints de observabilidad (`/api/admin/telemetry`) e inyección de payloads en logs estructurados.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Modelado inicial del atacante cibernético externo. |
