---
id: FR-TENANT-001
type: requirement
title: Aislamiento Multi-Tenant de Centros de Estética por Cliente
status: active
version: 1.0.0
schema-version: '1.0'
category: functional
derives-from:
  - UC-COST-CATALOG-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - '@FR-TENANT-001'
  - '@automated'
  - '@regression'
supersedes: null
superseded-by: null
---


# Requirement: Aislamiento Multi-Tenant de Centros de Estética por Cliente

## 1. Normative Statement

El sistema DEBE permitir a cada cliente autenticado crear y gestionar uno o varios centros de estética bajo su titularidad —incluyendo la opción de copiar el catálogo de productos entre centros pertenecientes al mismo cliente propietario (Mejora M4)—, garantizando mediante controles de autorización en servidor que ningún usuario pueda visualizar, copiar, modificar, recalcular ni exportar costes, escandallos o informes pertenecientes a centros de estética de otros clientes.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-TENANT-001 @automated @regression
Feature: Aislamiento Multi-Tenant de Centros de Estética por Cliente

  Scenario: Usuario con múltiples centros copia su catálogo de productos entre sus propios centros manteniendo el aislamiento frente a terceros
    Given el usuario "cliente-a@estetica.es" es propietario de "Centro Madrid" y "Centro Valencia"
    And el usuario "cliente-b@estetica.es" es propietario de "Centro Sevilla"
    When "cliente-a@estetica.es" solicita copiar el catálogo de productos desde "Centro Madrid" hacia "Centro Valencia"
    Then el servidor copia los productos hacia "Centro Valencia" con código HTTP 200
    And los datos de "Centro Sevilla" permanecen inaccesibles para "cliente-a@estetica.es"

  Scenario Outline: Denegación de acceso o copia cruzada entre centros de distintos clientes
    Given el usuario "<usuario_autenticado>" está autenticado en el servidor
    And el centro "<centro_objetivo>" pertenece al propietario "<propietario_real>"
    When "<usuario_autenticado>" intenta ejecutar la acción "<accion>" sobre "<centro_objetivo>"
    Then el servidor responde con código HTTP <codigo_http>

    Examples:
      | usuario_autenticado   | centro_objetivo | propietario_real      | accion                     | codigo_http |
      | cliente-a@estetica.es | Centro Madrid   | cliente-a@estetica.es | consultar_catalogo_costes  | 200         |
      | cliente-a@estetica.es | Centro Valencia | cliente-a@estetica.es | copiar_catalogo_propio     | 200         |
      | cliente-a@estetica.es | Centro Sevilla  | cliente-b@estetica.es | consultar_catalogo_costes  | 403         |
      | cliente-b@estetica.es | Centro Madrid   | cliente-a@estetica.es | copiar_catalogo_ajeno      | 403         |

  @security @mitigation
  Scenario Outline: Rechazo controlado de identificadores de centro fuera de rango, vacíos o no autorizados
    Given una petición hacia el endpoint multi-tenant con sesión "<estado_sesion>"
    When se envía el identificador de centro "<center_id_invalido>" para la operación "<operacion>"
    Then el servidor rechaza la operación de forma controlada con código HTTP <codigo_error> sin exponer trazas internas

    Examples:
      | estado_sesion | center_id_invalido                   | operacion          | codigo_error |
      | autenticado   | ""                                   | listar_escandallos | 400          |
      | autenticado   | "not-a-valid-uuid-format"            | copiar_catalogo    | 400          |
      | anonimo       | "550e8400-e29b-41d4-a716-4466554400" | listar_escandallos | 401          |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-04 | agent-product-analyst | Definición inicial del requisito de aislamiento multi-centro y multi-cliente. |
| 1.2.0 | 2026-10-05 | agent-product-analyst | Incorporación de Mejora M4 (copia de catálogo entre centros del mismo propietario). |


## Historial de Revisiones

| Versión | Fecha | Autor / Agente | Descripción del Cambio | Referencia de Cambio (Change/PR) |
| :--- | :--- | :--- | :--- | :--- |
| **1.0.0** | 2026-10-07 | agent-devops | Implementación verificada (TSK-001, TSK-002, TSK-003, TSK-004, TSK-005, TSK-006, TSK-007) | chg-001-escandallo-mvp |
