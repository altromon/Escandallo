---
id: SEC-REQ-DOS-001
type: security-requirement
title: Rate Limiting por Inquilino, Cuotas de Concurrencia en Exportación PDF/Excel y Límites de Recálculo
status: active
version: 1.0.0
schema-version: '1.0'
category: security
priority: high
derives-from:
  - UC-COST-CATALOG-001
  - UC-ESCANDALLO-CALC-001
  - UC-EXEC-REPORT-001
mitigates-abuse-case:
  - ABUSE-DOS-CASCADE-001
enforced-in-enclave: SEC-ENC-EDGE-WAF-001
owasp-asvs: V1.11, V11.1, V13.1
stride-category: Denial of Service
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - '@SEC-REQ-DOS-001'
  - '@automated'
  - '@regression'
supersedes: null
superseded-by: null
---


# Security Requirement: Rate Limiting por Inquilino, Cuotas de Concurrencia en Exportación PDF/Excel y Límites de Recálculo

## 1. Normative Statement

Para proteger la rentabilidad y disponibilidad del servidor frente a ataques de denegación de servicio asimétrica (`ABUSE-DOS-CASCADE-001`, OWASP ASVS V1.11, V11.1, V13.1), el sistema **DEBE** aplicar controles de estrangulamiento (*throttling*) en el perímetro (`SEC-ENC-EDGE-WAF-001`) y en el núcleo (`SEC-ENC-APP-CORE-001`):
1. **Límite de Frecuencia en Mutaciones con Recálculo en Cascada**: Cada inquilino (`user_id` / `center_id`) **DEBE** estar limitado a un máximo de `30 mutaciones de catálogo por minuto` que disparen recálculos en cascada (`BR-RECALC-HISTORY-001`), evitando la inflación maliciosa de snapshots históricos.
2. **Semáforo de Concurrencia y Rate Limit en Generación Documental (`PDF`/`XLSX`)**: La exportación de informes ejecutivos (`UC-EXEC-REPORT-001`) **DEBE** limitarse a un máximo de `2 generaciones concurrentes` y `10 exportaciones por minuto` por centro de estética, con un timeout duro de ejecución de `10 segundos` por documento.
3. **Límite de Cardinalidad por Receta**: Un escandallo individual **NO DEBE** superar un máximo de `100 líneas de coste totales` en el constructor dinámico (`M3`), rechazando payloads sobredimensionados con código HTTP `413 Payload Too Large` o `429 Too Many Requests`.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@SEC-REQ-DOS-001 @automated @regression @security @mitigation
Feature: Protección contra denegación de servicio asimétrica en recálculos y generación de informes
  Como pasarela perimetral y gestor de recursos (SEC-ENC-EDGE-WAF-001)
  Quiero limitar la tasa de recálculos en cascada y la concurrencia de renderizado PDF/Excel por inquilino
  Para garantizar la estabilidad del servidor compartido frente a vecinos ruidosos y bots

  Scenario: Bloqueo de ráfaga de exportaciones concurrentes de informes PDF por un mismo inquilino
    Given el usuario del centro "C_ACTIVO" tiene 2 exportaciones de informe PDF en curso simultáneamente
    When el mismo usuario envía una tercera petición concurrente de exportación PDF antes de que finalicen las anteriores
    Then el sistema rechaza la petición excedente con estado HTTP 429
    And incluye la cabecera "Retry-After" indicando el tiempo de espera recomendado
    And el consumo de memoria y CPU del resto de centros de estética permanece dentro del umbral operativo

  Scenario Outline: Aplicación de límites de tasa y cardinalidad ante tráfico abusivo
    Given un cliente autenticado en el centro "C_ACTIVO" ejecuta la operación "<operacion_intensiva>"
    When supera el umbral permitido de "<umbral_configurado>" enviando "<volumen_enviado>"
    Then el sistema intercepta el exceso devolviendo el código HTTP <codigo_http>
    And se registra una alerta preventiva con severidad "<severidad_log>"

    Examples:
      | operacion_intensiva                  | umbral_configurado               | volumen_enviado            | codigo_http | severidad_log |
      | actualizacion_coste_con_cascada      | 30 mutaciones por minuto         | 45 mutaciones en 20s       | 429         | WARN          |
      | exportacion_informe_ejecutivo_xlsx   | 10 exportaciones por minuto      | 15 exportaciones en 30s    | 429         | SECURITY      |
      | guardado_receta_escandallo_m3        | 100 lineas maximas por servicio  | 500 lineas en un servicio  | 422         | SECURITY      |

  @performance
  Scenario Outline: Análisis de frontera exacta (N-1, N, N+1) en concurrencia de informes, mutaciones por minuto y líneas de receta
    Given un centro activo tiene <export_concurrentes> exportaciones en curso, <mutaciones_min> mutaciones en el minuto actual y envía una receta de <lineas_receta> líneas
    When el limitador de recursos evalúa la petición entrante
    Then el servidor devuelve código HTTP <codigo_http> con estado "<estado_throttling>"

    Examples:
      | export_concurrentes | mutaciones_min | lineas_receta | codigo_http | estado_throttling                 |
      | 1                   | 29             | 99            | 200         | DENTRO_DE_UMBRAL                  |
      | 2                   | 30             | 100           | 200         | EN_LIMITE_EXACTO_PERMITIDO        |
      | 3                   | 10             | 50            | 429         | CONCURRENCIA_EXPORTACION_EXCEDIDA |
      | 1                   | 31             | 50            | 429         | TASA_MUTACIONES_CASCADA_EXCEDIDA  |
      | 1                   | 10             | 101           | 422         | CARDINALIDAD_RECETA_EXCEDIDA      |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-05 | agent-threat-modeler | Especificación inicial de límites de tasa y concurrencia anti-DoS. |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-07 | agent-devops | Implementación verificada (TSK-001, TSK-002, TSK-003, TSK-004, TSK-005, TSK-006, TSK-007) | chg-001-escandallo-mvp |
