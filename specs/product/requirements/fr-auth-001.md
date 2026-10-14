---
id: FR-AUTH-001
type: requirement
title: "Registro de Cuenta de Usuario, Inicio de Sesión Obligatorio y Gestión de Centros por Titular"
status: draft
version: "1.1.0"
schema-version: "1.0"
category: functional
derives-from:
  - UC-AUTH-001
verifiable-by: cucumber-bdd
acceptance-format: gherkin
cucumber-tags:
  - "@FR-AUTH-001"
  - "@automated"
  - "@regression"
supersedes: null
superseded-by: null
---

# Requirement: Registro de Cuenta de Usuario, Inicio de Sesión Obligatorio y Gestión de Centros por Titular

## 1. Normative Statement

El sistema DEBE solicitar de forma obligatoria el inicio de sesión o la creación de una cuenta de usuario antes de mostrar o permitir cualquier operación sobre catálogos de costes, escandallos o informes. El sistema DEBE permitir registrar nuevas cuentas de cliente creando automáticamente su primer centro de estética, iniciar sesión validando credenciales en servidor, disponer de un botón visible de **Cerrar sesión** en la cabecera que revoque la sesión activa en servidor y devuelva al usuario a la pantalla de inicio de sesión ocultando el espacio de trabajo, contar con las cuentas de demostración de referencia (`cliente-a@estetica.es` y `cliente-b@estetica.es`) para verificación multi-centro y permitir a cada cliente autenticado dar de alta centros adicionales bajo su exclusiva titularidad.

## 2. Acceptance Criteria (Gherkin)

```gherkin
@FR-AUTH-001 @automated @regression
Feature: Registro de Cuenta de Usuario, Inicio de Sesión Obligatorio y Gestión de Centros por Titular

  Scenario: Registro de una nueva cuenta de cliente con creación automática de su primer centro de estética
    Given un visitante accede por primera vez a la pantalla de acceso de la aplicación sin sesión activa
    When completa el registro de cuenta con correo "nuevo@estetica.es", contraseña válida y nombre de centro "Estética Serrano"
    Then el servidor crea la cuenta y su centro inicial respondiendo con código HTTP 201
    And la sesión queda autenticada mostrando únicamente el centro "Estética Serrano" bajo la titularidad de "nuevo@estetica.es"

  Scenario Outline: Inicio de sesión con credenciales válidas, cierre de sesión, rechazo de credenciales incorrectas y bloqueo de intentos de escalada de rol
    Given el sistema de autenticación recibe una solicitud para el correo "<correo>" con la operación "<operacion>"
    When el servidor verifica las credenciales y políticas de seguridad de la cuenta
    Then el servidor responde con código HTTP <codigo_http> y estado de sesión "<estado_sesion>"

    Examples:
      | correo                | operacion                      | codigo_http | estado_sesion          |
      | cliente-a@estetica.es | login_credenciales_validas     | 200         | AUTENTICADO_CLIENTE_A  |
      | cliente-b@estetica.es | login_credenciales_validas     | 200         | AUTENTICADO_CLIENTE_B  |
      | cliente-a@estetica.es | logout_sesion_activa           | 200         | SESION_CERRADA         |
      | cliente-a@estetica.es | login_password_incorrecto      | 401         | RECHAZADO_CREDENCIALES |
      | atacante@estetica.es  | registro_con_rol_maintainer    | 403         | BLOQUEADO_OVERPOSTING  |
```

## 3. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-09 | agent-product-analyst | Especificación del requisito funcional de login obligatorio, registro de cuenta y alta de centros propios. |
| 1.1.0 | 2026-10-09 | agent-product-analyst | Inclusión del botón de cierre de sesión (logout) con revocación en servidor y retorno a la pantalla de login. |
