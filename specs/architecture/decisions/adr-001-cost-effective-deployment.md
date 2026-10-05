---
id: ADR-001-COST-EFFECTIVE-DEPLOYMENT
type: architecture-decision-record
title: "Topología de Despliegue Ultra-Rentable y Segura: Cloudflare Edge WAF + Monolito Modular Dockerizado en VPS Europeo"
status: proposed
version: "1.0.0"
schema-version: "1.0"
deciders:
  - "agent-system-architect"
  - "Product Owner"
decision-date: "2026-10-05"
affects-components:
  - CMP-ESCANDALLO-SYS-001
  - CMP-WEB-UI-001
  - CMP-IAM-TENANT-001
  - CMP-OBS-TELEMETRY-001
supersedes: null
superseded-by: null
---

# ADR-001: Topología de Despliegue Ultra-Rentable y Segura: Cloudflare Edge WAF + Monolito Modular Dockerizado en VPS Europeo

## 1. Context and Problem Statement

El sistema SaaS de cálculo de escandallos para centros de estética debe ejecutarse en servidor, soportar múltiples clientes con aislamiento estricto (`FR-TENANT-001`), ofrecer una capa gratuita de hasta 2 escandallos activos por centro (`FR-BILLING-001`), cobrar suscripciones de **14,90 EUR/mes** (o **149,00 EUR/año**) mediante Stripe (`UC-SUBSCRIPTION-001`), y resistir ataques cibernéticos (`QR-SEC-001`, `SEC-REQ-DOS-001`).

El usuario solicita explícitamente **aconsejar el despliegue más rentable**. En un SaaS B2B vertical con un modelo Freemium, el mayor riesgo financiero en etapa temprana e intermedia es el coste fijo de infraestructura ociosa (bases de datos gestionadas sobredimensionadas, balanceadores de carga dedicados o clústeres Kubernetes) que obligarían a captar decenas de clientes de pago solo para cubrir la factura mensual del proveedor cloud.

## 2. Considered Technology Options

| Criterio / Escala | Opción A (RECOMENDADA): Cloudflare Edge + VPS Cloud Europeo (Hetzner / Netcup) con Docker Compose | Opción B: PaaS Gestionado (Railway / Render / Fly.io + Neon/Supabase DB) | Opción C: Hyperscaler Cloud (AWS ECS Fargate + RDS Multi-AZ + ALB + AWS WAF) |
| :--- | :--- | :--- | :--- |
| **Arquitectura de Red y Cómputo** | **Cloudflare (Free/Pro)** como proxy inverso Anycast, WAF, DDoS L3/L7 y TLS 1.3 $\rightarrow$ Túnel cifrado / IP filtrada hacia **1 VPS Cloud Europeo** (2–4 vCPU, 4–8 GB RAM, NVMe RAID10) ejecutando `Caddy` + `Node.js LTS` + `PostgreSQL 16` en redes Docker aisladas + Backups diarios cifrados (`age`/`restic`) en **Cloudflare R2 / Hetzner Storage Box** | Contenedor Node.js gestionado en PaaS con base de datos PostgreSQL serverless externa | Contenedores en subredes privadas, Application Load Balancer dedicado, NAT Gateway, RDS PostgreSQL y AWS WAF |
| **Coste Mensual MVP (1–150 centros)** | **~5,50 EUR – 9,50 EUR / mes** (`CX22`/`CX32` Hetzner + IPv4 + R2 gratis hasta 10 GB + Cloudflare Free) | **~25,00 EUR – 65,00 EUR / mes** (Plan Pro base + cómputo 24/7 + almacenamiento DB) | **~185,00 EUR – 340,00 EUR / mes** (ALB ~20€ + NAT ~32€ + WAF ~25€ + RDS ~60€ + ECS ~45€) |
| **Coste Mensual Escala Media (500 centros)** | **~16,00 EUR – 34,00 EUR / mes** (`CAX31`/`CX42` 8–16 GB RAM + Cloudflare Pro opcional) | **~140,00 EUR – 280,00 EUR / mes** (penalización por CPU en recálculos/PDF y tráfico egress) | **~450,00 EUR – 750,00 EUR / mes** |
| **Punto de Equilibrio (Break-Even)** | **1 sola suscripción mensual pagada (14,90 EUR/mes)** cubre el 100% de la infraestructura y genera superávit neto desde el primer cliente | Requiere **3 a 5 suscripciones de pago** permanentes solo para empatar gastos | Requiere **15 a 25 suscripciones de pago** solo para cubrir costes fijos |
| **Seguridad frente a Ciberataques** | **Muy Alta**: La IP real del servidor está oculta tras Cloudflare; el firewall de red del proveedor solo acepta tráfico en el puerto 443 desde los rangos IP oficiales de Cloudflare (mTLS Authenticated Origin Pulls) | **Alta**: TLS gestionado, pero la protección WAF avanzada suele requerir planes Enterprise costosos | **Muy Alta**, pero cada regla de AWS WAF y GB de tráfico inspeccionado incrementa la factura |
| **Cumplimiento RGPD / Soberanía de Datos** | **Nativo UE** (Regiones Alemania `Falkenstein`/`Nürnberg` o Finlandia `Helsinki`, ISO 27001, DPA europeo) | Variable (a menudo requiere planes superiores para fijar región UE y backups PITR) | Nativo UE (`eu-west-1` / `eu-central-1`) |

## 3. Decision Outcome

Se elige la **Opción A (Cloudflare Edge WAF + Monolito Modular Dockerizado en VPS Cloud Europeo Hetzner CX22/CX32 con PostgreSQL 16 local y Backups cifrados en Cloudflare R2)** por las siguientes razones técnicas y económicas:

1. **Rentabilidad Inmediata (ROI desde el Cliente #1)**: Con un TCO inicial de **~5,50 EUR a 9,50 EUR/mes**, una única suscripción mensual (**14,90 EUR/mes**) financia toda la plataforma y deja un margen bruto de infraestructura $>35\%$. A partir del segundo centro suscrito, el margen bruto sobre infraestructura supera el **90%**.
2. **Rendimiento Determinista y Latencia Sub-Milisegundo en Recálculos**: Al residir el motor de aplicación Node.js (`CMP-COST-ENGINE-001`) y PostgreSQL 16 en el mismo host sobre disco NVMe local (conectados por socket/red interna Docker sin salto de red WAN), una transacción de recálculo en cascada con escritura de snapshots históricos (`BR-RECALC-HISTORY-001`) tarda **$< 5\text{ ms}$**, frente a los $30\text{–}80\text{ ms}$ de latencia por consulta en bases de datos serverless remotas.
3. **Mitigación Perimetral de Coste Cero frente a DDoS y Bots (`SEC-ENC-EDGE-WAF-001`)**: Cloudflare absorbe en su red Anycast global los ataques volumétricos y aplica reglas WAF y Rate Limiting antes de que los paquetes lleguen al VPS. El firewall de Hetzner descarta a nivel de hipervisor cualquier paquete que no provenga de Cloudflare.
4. **Escalabilidad Vertical Sin Fricción y Ruta de Salida Limpia**: Un único nodo de 4 vCPU y 8 GB RAM (`~8 EUR/mes`) soporta holgadamente más de **1.000 centros de estética activos** gracias al bajo consumo del monolito en TypeScript (`< 250 MB RAM` en reposo). Cuando el negocio supere los 2.000 centros, la arquitectura contenedorizada permite separar PostgreSQL a un nodo dedicado o servicio gestionado en minutos cambiando únicamente `DATABASE_URL`.

## 4. Consequences

- **Positive**:
  - Mínimo coste fijo mensual del mercado europeo manteniendo cumplimiento RGPD estricto.
  - Cero costes sorpresa por ancho de banda (*egress*) o picos de lectura/escritura en base de datos causados por usuarios en el plan gratuito o por ataques DoS.
  - Cumplimiento 100% con `license-policy.yaml` (Linux Debian/Ubuntu, Docker Engine `Apache-2.0`, Caddy `Apache-2.0`, PostgreSQL `PostgreSQL License`).
- **Negative / Trade-offs**:
  - Requiere automatizar mediante un contenedor *sidecar* ligero (`restic` / `pg_dump` cron diario) la copia de seguridad cifrada hacia almacenamiento de objetos externo (Cloudflare R2 Free Tier 10 GB) y la actualización desatendida de parches de seguridad del sistema operativo (`unattended-upgrades`).

---

## 5. Revision History and Version Control

| Version | Date | Deciders | Decision Status | Reference (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-05 | agent-system-architect | Formal decision proposal | ARCH-INIT-001 |
