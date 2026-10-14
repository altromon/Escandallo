---
id: ARCH-RISK-001
type: risks-and-technical-debt
title: "11. Riesgos Arquitectónicos y Deuda Técnica (arc42 Sec. 11 / NAF Risk & Debt)"
status: proposed
version: "1.1.0"
schema-version: "1.0"
arc42-section: 11
naf-perspective: "Risk & Technical Debt"
risks:
  - RSK-BLAZOR-CIRCUIT-001
  - RSK-SINGLE-VPS-SPOF-002
  - RSK-DOC-MEM-SPIKE-003
  - RSK-RLS-POOL-LEAK-004
supersedes: null
superseded-by: null
---

# 11. Riesgos y Deuda Técnica (arc42 Sec. 11 / NAF Risk & Debt)

## 1. Matriz de Riesgos Arquitectónicos (`RSK-*`)

Evaluación y estrategia de mitigación de los riesgos técnicos derivados del stack **.NET 10 LTS + Blazor** y del despliegue ultra-rentable:

| ID Riesgo | Descripción del Riesgo Técnico | Probabilidad | Impacto | Estrategia de Mitigación Arquitectónica |
| :--- | :--- | :---: | :---: | :--- |
| `RSK-BLAZOR-CIRCUIT-001` | Consumo de memoria en el servidor o manipulación de parámetros si usuarios o atacantes abren múltiples circuitos SignalR (`/_blazor`) en Blazor Interactive Server. | Media | Medio | Usar **Blazor SSR** por defecto y activar modo interactivo solo en el simulador (`EscandalloBuilderPage.razor`, máx. `100` líneas), imponiendo `DisconnectedCircuitRetentionPeriod = 3 min`, `MaxBufferedUnacknowledgedRenderBatches = 10`, `MaximumReceiveMessageSize = 32 KB`, límite de `5 circuitos simultáneos por usuario` (`SEC-REQ-DOS-001`) y revalidación continua de `AuthenticationStateProvider` en servidor (`SEC-REQ-TENANT-001`). |
| `RSK-SINGLE-VPS-SPOF-002` | Punto único de fallo (SPOF) de hardware en la Fase 1 al alojar aplicación y PostgreSQL en un único nodo Cloud VPS europeo para minimizar costes (`< 12 €/mes`). | Baja | Alto | Snapshots automáticos diarios y envío continuo de WAL/backups cifrados (`AES-256`) a Object Storage S3 externo (`RES-STORAGE-BACKUP-001`), permitiendo reconstruir el nodo completo mediante `docker compose up -d` en `< 15 minutos` (RTO < 15m, RPO < 1h). |
| `RSK-DOC-MEM-SPIKE-003` | Picos de asignación en el *Large Object Heap (LOH)* de .NET 10 durante exportaciones simultáneas `.xlsx`/`.pdf` o subida de archivos `.xlsx` con cabeceras ZIP falsificadas. | Media | Medio | Acotar concurrencia con `SemaphoreSlim(2, 2)` y `10 exportaciones/min` (`SEC-REQ-DOS-001`), descomprimir mediante `BoundedReadStream` ($\le 10\text{ MB}$ reales, $\le 5.000$ filas, `SEC-REQ-XLSX-001`) y reutilizar buffers con `Microsoft.IO.RecyclableMemoryStream` (`MIT`). |
| `RSK-RLS-POOL-LEAK-004` | Contaminación del contexto `app.current_center_id` entre peticiones si una conexión devuelta al pool de `Npgsql` conserva variables de sesión del tenant anterior o el rol tiene `BYPASSRLS`. | Baja | Crítico | Conectar con un rol PostgreSQL 16 estrictamente `NOSUPERUSER NOBYPASSRLS` y ejecutar exclusivamente `SELECT set_config('app.current_center_id', @centerId::text, true)` (`SET LOCAL`, cuyo alcance muere al hacer `COMMIT`/`ROLLBACK`) bajo políticas `ENABLE` + `FORCE ROW LEVEL SECURITY` *fail-closed* (`SEC-REQ-TENANT-001`). |

---

## 2. Registro de Deuda Técnica y Compromisos Deliberados (Trade-offs MVP)

| Elemento de Deuda / Compromiso | Componente Afectado | Justificación del Trade-off (Simplicidad / Coste) | Plan de Amortización / Evolución Futura |
| :--- | :--- | :--- | :--- |
| **Recálculo en Cascada Síncrono en Proceso (`<= 200` escandallos)** | `CMP-COST-ENGINE-001` | Evita desplegar y mantener un broker externo (RabbitMQ/Redis) en el MVP, garantizando consistencia ACID inmediata en `< 2000 ms`. | Si un centro supera los `200` tratamientos vinculados a un mismo insumo, migrar el excedente a una cola en proceso basada en `System.Threading.Channels<T>` con notificación de progreso. |
| **Despliegue Mononodo Docker Compose en Fase 1** | `ARCH-DEP-001` | Reduce el coste fijo de infraestructura a `~6,50 - 9,90 €/mes`, logrando rentabilidad desde el 1.er cliente de pago (`ADR-001-COST-EFFECTIVE-DEPLOYMENT`). | Al superar `500` centros activos o `2.000 €/mes` de MRR, separar PostgreSQL a una instancia dedicada con réplica en espera (*warm standby*). |

---

## 3. Historial de Revisiones y Control de Versiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-04-12 | `agent-system-architect` | Identificación inicial de riesgos técnicos (.NET/Blazor/VPS) y compromisos de deuda | `chg-001-escandallo-mvp` |
| **1.1.0** | 2026-10-08 | `agent-system-architect` | Incorporación de mitigaciones técnicas v1.1.0 para `RSK-BLAZOR-CIRCUIT-001`, `RSK-DOC-MEM-SPIKE-003` y `RSK-RLS-POOL-LEAK-004` | `chg-001-escandallo-mvp` |
