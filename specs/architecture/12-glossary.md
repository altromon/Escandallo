---
id: ARCH-GLOSS-001
type: architecture-glossary
title: "12. Glosario de Arquitectura y Taxonomía de Términos (arc42 Sec. 12 / NAF Taxonomy)"
status: proposed
version: "1.1.0"
schema-version: "1.0"
arc42-section: 12
naf-perspective: "Taxonomy & Terms"
cites-domain-terms:
  - TERM-ESCANDALLO-001
cites-bounded-contexts:
  - "costing-and-pricing"
supersedes: null
superseded-by: null
---

# 12. Glosario de Arquitectura (arc42 Sec. 12 / NAF Taxonomy)

## 1. Términos Canónicos de Dominio (`TERM-*`)

Definiciones oficiales del lenguaje ubicuo compartidas entre las especificaciones de producto (`ProductShape`), el modelo de amenazas y el código fuente en C# (`.NET 10 LTS`):

| Término / ID Canónico | Definición Canónica en el Dominio | Equivalente en Código C# (`costing-and-pricing`) |
| :--- | :--- | :--- |
| [`TERM-ESCANDALLO-001`](file:///c:/Users/reypo/Documents/Workspace/Escandallo/specs/product/terms/term-escandallo-001.md) | Estructura analítica y desglose exacto de todos los costes directos e indirectos (consumibles con merma, amortización de aparatología, mano de obra por minuto, gastos fijos y comisión TPV) imputables a una sesión de un tratamiento estético para determinar su margen neto real. | `EscandalloAggregate`, `EscandalloCalculationResult` |
| **Merma Operativa (`WastePct`)** | Porcentaje de producto cosmético o desechable que se pierde inevitablemente durante la preparación o aplicación en cabina (ej. producto adherido al bol, espátula o jeringa). | `decimal DefaultWastePct` (`NUMERIC(18,6)`) |
| **Semáforo de Erosión de Margen** | Indicador visual y accesible (`🟢 Óptimo`, `🟡 En Riesgo`, `🔴 Crítico`) que compara el margen neto porcentual real frente al margen objetivo configurado por la directora del centro. | `enum MarginSemaphoreState { Green, Yellow, Red }` |
| **Copia Dual Controlada (M4)** | Operación mediante la cual una propietaria con múltiples centros clona por valor un catálogo o escandallo desde un centro origen hacia un centro destino, previa verificación de titularidad explícita en ambos. | `ITenantIsolationGuard.AuthorizeDualCenterCopyAsync` |
| **Archivado Lógico de Escandallo (M1)** | Cambio de estado reversible (`active` $\leftrightarrow$ `archived`) que libera cupo en el límite de 2 escandallos activos del plan gratuito sin destruir el historial de versiones (`BR-FREEMIUM-QUOTA-001`). | `enum EscandalloLifecycleStatus { Active, Archived }` |

---

## 2. Abreviaturas y Acrónimos Técnicos

| Acrónimo | Significado Completo | Definición en el Contexto del Sistema |
| :--- | :--- | :--- |
| **Blazor SSR** | Blazor Server-Side Rendering | Renderizado HTML estático/streaming en ASP.NET Core 10 que entrega páginas instantáneas con huella mínima de memoria. |
| **RLS (`SET LOCAL`)** | Row-Level Security Transaccional | Mecanismo nativo de PostgreSQL 16 (`FORCE ROW LEVEL SECURITY` bajo rol `NOBYPASSRLS`) que restringe qué filas puede leer o escribir cada transacción según `set_config('app.current_center_id', @id, true)`. |
| **TOCTOU** | Time-of-Check to Time-of-Use | Condición de carrera entre la verificación de una regla (ej. cupo Freemium de 2 escandallos) y su escritura, mitigada con bloqueo pesimista `SELECT ... FOR UPDATE`. |
| **BoundedReadStream** | Stream Contador Acotado Anti-Zip-Bomb | Envoltorio sobre `RecyclableMemoryStream` que cuenta los bytes reales descomprimidos al vuelo y aborta si superan `10 MB`, neutralizando cabeceras `ZipArchiveEntry.Length` falsificadas (`SEC-REQ-XLSX-001`). |
| **DDE / CSV Injection** | Dynamic Data Exchange Injection | Ataque en el que celdas de texto que comienzan (tras `TrimStart()`) por `=`, `+`, `-`, `@`, `\t` o `\r` ejecutan comandos al abrirse en Excel; mitigado anteponiendo `'` y fijando `XLDataType.Text`. |
| **OTLP** | OpenTelemetry Protocol | Protocolo estándar abierto (`Apache-2.0`) utilizado por `CMP-OBS-TELEMETRY-001` para exportar trazas, métricas y logs sanitizados. |

---

## 3. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-04-12 | `agent-system-architect` | Definición inicial del glosario de arquitectura y mapeo al código C# | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Incorporación de términos técnicos de seguridad v1.1.0 (`BoundedReadStream`, `SET LOCAL RLS`, `XLDataType.Text`) | `chg-001-escandallo-mvp` |
