---
id: ADR-001-COST-EFFECTIVE-DEPLOYMENT
type: architecture-decision-record
title: "Topología de Despliegue Ultra-Rentable: Cloudflare Edge WAF + Contenedores .NET 10 LTS y PostgreSQL 16 en Cloud VPS Europeo"
status: proposed
version: "1.1.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "Product Owner"
decision-date: "2026-10-08"
affects-components:
  - CMP-ESCANDALLO-SYS-001
  - CMP-WEB-UI-001
  - CMP-IAM-TENANT-001
  - CMP-OBS-TELEMETRY-001
supersedes: null
superseded-by: null
---

# ADR-001: Topología de Despliegue Ultra-Rentable: Cloudflare Edge WAF + Contenedores .NET 10 LTS y PostgreSQL 16 en Cloud VPS Europeo

## 1. Contexto y Planteamiento del Problema

La plataforma SaaS de Escandallos para Centros de Estética opera bajo un modelo Freemium (`BR-FREEMIUM-QUOTA-001`: hasta 2 escandallos activos gratis por centro) y una suscripción Pro accesible para pequeños salones y clínicas (`~14,90 € - 29,00 €/mes`). Necesitamos una arquitectura de despliegue que satisfaga simultáneamente:
1. **Aislamiento de red en 3 enclaves** (`SEC-ENC-EDGE-WAF-001`, `SEC-ENC-APP-CORE-001`, `SEC-ENC-DATA-OBS-001`) y protección anti-DDoS asimétrico sobre el techo de `2 GB` de RAM del contenedor (`SEC-REQ-DOS-001`).
2. **Cumplimiento estricto RGPD** con residencia de datos dentro de la Unión Europea (`QR-SEC-001`).
3. **Mínimo Coste Total de Propiedad (TCO)** para alcanzar *break-even* desde el primer cliente de pago sin sacrificar rendimiento (`< 100 ms` en simulación Blazor, `< 2000 ms` en cascada de 200 escandallos).

## 2. Opciones Tecnológicas Consideradas

1. **Opción A (Recomendada — Cloud VPS Europeo + Docker Compose + Cloudflare Edge WAF)**:
   - Nodo Hetzner Cloud EU (Falkenstein/Nuremberg, `CX22` o `CPX21`, 4 vCPU, 8 GB RAM, 80 GB NVMe) ejecutando Ubuntu 24.04 LTS endurecido con Docker Compose (`Caddy` + `.NET 10 LTS Chiseled` + `PostgreSQL 16 Alpine`) detrás de **Cloudflare Edge WAF** (`mTLS Strict Origin Pulls`) y backups diarios cifrados (`AES-256`) en Object Storage S3 europeo.
   - **Coste fijo mensual**: **~6,50 € – 9,90 € / mes** (todo incluido).
2. **Opción B (PaaS Gestionado en Hiperescalar — Azure App Service + Azure Database for PostgreSQL + Azure Front Door)**:
   - Servicios gestionados nativos en Azure West Europe.
   - **Coste fijo mensual**: **~185,00 € – 260,00 € / mes** (requiere > 15 clientes de pago solo para cubrir infraestructura base).
3. **Opción C (Kubernetes Gestionado — AKS / EKS)**:
   - Clúster multi-nodo con plano de control y balanceadores de carga dedicados.
   - **Coste fijo mensual**: **> 220,00 € / mes** más elevada complejidad operativa.

## 3. Decisión Adoptada

Se elige la **Opción A (Cloudflare Edge WAF + Monolito Modular Dockerizado en Cloud VPS Europeo Hetzner EU con PostgreSQL 16 y Backups S3 Cifrados)**.

**Justificación técnica, económica y de seguridad perimetral (`SEC-ENC-EDGE-WAF-001`, `SEC-REQ-DOS-001`)**:
- **Rentabilidad Inmediata**: Con un coste total inferior a `10 €/mes`, una sola suscripción Pro (`14,90 €/mes`) cubre el 100% de la infraestructura.
- **Eficiencia Nativa de .NET 10 LTS**: Un único proceso `Kestrel` sirviendo Blazor SSR/Interactive y Minimal APIs consume `< 250 MB` de RAM en reposo y soporta más de `500` centros activos en `4 vCPU / 8 GB RAM` con holgura.
- **Aislamiento de Origen mTLS, `KnownProxies` y Cotas Perimetrales**: El firewall del VPS (`ufw`) bloquea todo tráfico entrante salvo el puerto `:443` desde los rangos de Cloudflare con verificación obligatoria de certificado cliente mTLS (*Authenticated Origin Pulls*) en Caddy. ASP.NET Core 10 configura `ForwardedHeadersMiddleware` confiando exclusivamente en el proxy Caddy interno (`KnownProxies`) para impedir el spoofing de `X-Forwarded-For` / `CF-Connecting-IP` en el `RateLimiter` (`SEC-REQ-DOS-001`). Caddy restringe además cuerpos `multipart/form-data` a $\le 2\text{ MB}$ (`SEC-REQ-XLSX-001`), webhooks de Stripe a $\le 64\text{ KB}$ (`SEC-REQ-BILLING-001`) y conexiones WebSocket `/_blazor`, mientras PostgreSQL 16 corre en la red `net-data-isolated` (`internal: true`, `ports: []`).

## 4. Consecuencias

- **Positivas**:
  - Ahorro superior al **94%** en gasto cloud durante la fase de validación y tracción inicial.
  - Latencia mínima (`0 ms` de salto de red local entre `.NET 10 LTS` y `PostgreSQL 16`).
  - Imposibilidad de *bypass* directo a la IP del VPS ni de evasión de límites de tasa mediante cabeceras IP falsificadas.
- **Negativas / Compensaciones (Trade-offs)**:
  - Requiere automatizar copias de seguridad hacia S3 (`pgBackRest` / `pg_dump` cifrado) y actualizaciones desatendidas del host Linux (RTO `< 15 min` recreando el VPS con Terraform/Ansible y Docker Compose).

---

## 5. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Decisores | Estado de la Decisión | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-08 | `agent-system-architect` & Product Owner | Propuesta formal aceptada para diseño MVP | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` & Product Owner | Incorporación de mTLS Origin Pulls, `KnownProxies` anti-spoofing y cotas perimetrales (`SEC-REQ-DOS-001`, `SEC-ENC-EDGE-WAF-001`) | `chg-001-escandallo-mvp` |
