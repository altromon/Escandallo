---
id: ACT-THREAT-EXT-001
type: threat-actor
title: "Atacante Cibernético Externo y Botnet Automatizada"
status: draft
version: "1.1.0"
schema-version: "1.0"
category: "external-attacker"
skill-level: "high"
motivation: "Compromiso del servidor .NET 10, denegación de servicio por agotamiento de circuitos SignalR/LOH, fraude de webhooks y exfiltración de telemetría"
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

Adversario externo en Internet que ataca la superficie pública del perímetro (`SEC-ENC-EDGE-WAF-001`) y del host **ASP.NET Core 10.0 + Blazor Web App** (`SEC-ENC-APP-CORE-001`) buscando evadir Cloudflare hacia la IP del VPS en Hetzner, agotar el límite de `2 GB` de RAM del contenedor mediante inundación de circuitos SignalR (`WSS /_blazor`) o fragmentación del *Large Object Heap (LOH)*, falsificar webhooks de Stripe o escalar privilegios hacia `ACT-MAINTAINER-001`.

## 2. Attack Capabilities & Vectors

- **Bypass de WAF a IP de Origen y Spoofing de `X-Forwarded-For` (`SEC-ENC-EDGE-WAF-001`)**: Escaneo directo contra la IP del VPS para evitar Cloudflare o inyección de cabeceras `X-Forwarded-For` / `CF-Connecting-IP` falsas para burlar el `RateLimiter` de ASP.NET Core 10.
- **Inundación de Circuitos Blazor Interactive Server (`WSS /_blazor`) y LOH (`SEC-ENC-EDGE-WAF-001` / `SEC-ENC-APP-CORE-001`)**: Apertura masiva de circuitos WebSocket persistentes, envío de lotes de renderizado no reconocidos (*unacknowledged render batches*) y ráfagas de exportación `PDFsharp` / `ClosedXML` para provocar `OutOfMemoryException` en el contenedor de `2 GB` de RAM (`RSK-BLAZOR-CIRCUIT-001`, `RSK-DOC-MEM-SPIKE-003`).
- **Spoofing & Replay de Webhooks y Corrupción de Stream en Kestrel (`SEC-ENC-APP-CORE-001`)**: Envío de peticiones `POST /api/v1/billing/webhooks/stripe` con firmas HMAC inválidas, timestamps fuera de la ventana de `300s` o payloads gigantes para agotar memoria antes de `Stripe.EventUtility.ConstructEvent`.
- **Exploits sobre `ZipArchive` / `ClosedXML` y `System.Decimal` (`SEC-ENC-APP-CORE-001`)**: Subida de archivos `.xlsx` con tamaño declarado `0` en la cabecera ZIP Central Directory pero gigabytes de flujo DEFLATE real (*ZIP Header Spoofing*), entidades externas XML (*XXE*), fórmulas DDE o valores extremos para provocar `System.OverflowException` en cálculos de 128 bits.
- **Escalada a `ACT-MAINTAINER-001`, Bombas de Destructuración `Serilog` y Log Injection (`SEC-ENC-DATA-OBS-001`)**: Inyección de grafos profundos o saltos `\r\n` para corromper logs JSON, inundación del *ring-buffer* para expulsar alertas `SECURITY` y búsqueda de cadenas de conexión `Npgsql` en trazas de excepción cuando `LoggingLevelSwitch` está en `DEBUG`.

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Modelado inicial del atacante cibernético externo. |
| 1.1.0 | 2026-10-08 | agent-threat-modeler | Incorporación de vectores técnicos sobre .NET 10 LTS, circuitos Blazor SignalR, LOH, ZipArchive y Serilog. |
