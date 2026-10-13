---
id: UC-AUTH-001
type: use-case
title: "Registro de Cuenta de Cliente, Inicio de Sesión y Gestión de Centros Propios"
status: draft
version: "1.1.0"
schema-version: "1.0"
primary-actor: ACT-ESTHETIC-USER-001
governed-by:
  - BR-TENANT-ISOLATION-001
supersedes: null
superseded-by: null
---

# Use Case: Registro de Cuenta de Cliente, Inicio de Sesión y Gestión de Centros Propios

## 1. Intent and Outcome

As a `ACT-ESTHETIC-USER-001` I want to create a personal user account or sign in with my credentials before accessing the platform, and register one or multiple beauty centers under my ownership To protect my financial data and manage the cost catalogs and service escandallos of all my locations in an isolated workspace.

## 2. Preconditions

- El usuario accede a la aplicación web desde un navegador en ordenador, tablet o móvil sin una sesión activa o deseando cambiar de cuenta.

## 3. Main Flow

1. Al abrir la aplicación sin sesión activa, el sistema presenta de forma obligatoria la pantalla de acceso con dos opciones: **Iniciar sesión** con una cuenta existente o **Crear cuenta nueva** de cliente, manteniendo ocultos los catálogos, calculadoras e informes.
2. Si el cliente elige **Crear cuenta nueva**, introduce su nombre o titularidad, correo electrónico, contraseña y el nombre comercial de su primer centro de estética; el sistema valida los datos, crea la cuenta con perfil estándar de dirección de centro de estética, inicializa su primer centro con el catálogo base y abre la sesión autenticada.
3. Si el cliente elige **Iniciar sesión**, introduce su correo electrónico y contraseña; el sistema verifica las credenciales en servidor, establece la sesión autenticada y carga exclusivamente la lista de centros de estética pertenecientes a dicho titular.
4. Una vez dentro de la aplicación, el cliente autenticado puede alternar entre sus centros propios, dar de alta nuevos centros de estética bajo su misma cuenta, copiar el catálogo de productos entre sus propios centros o cerrar su sesión en cualquier momento.
5. Cuando el cliente pulsa el botón **Cerrar sesión** en la cabecera de la aplicación, el sistema invalida la sesión activa en servidor, elimina la credencial de sesión, limpia los datos financieros en pantalla y devuelve al usuario de inmediato a la pantalla de inicio de sesión.

## 4. Alternative and Exception Flows

- **E1 – Intento de acceso sin iniciar sesión**: Si un cliente no autenticado intenta consultar o modificar datos de centros, el servidor rechaza la petición con código HTTP `401 Unauthorized` y mantiene visible únicamente la pantalla de inicio de sesión y registro.
- **E2 – Credenciales inválidas o correo ya registrado**: El sistema informa del error de validación sin revelar información sensible de otros usuarios.
- **E3 – Intento de asignación no autorizada de rol administrativo en el registro**: Si la solicitud de registro incluye parámetros de rol administrativo (`role=maintainer`), el servidor rechaza la petición con código HTTP `403 Forbidden` conforme a `SEC-REQ-OBS-001`.

## 5. Postconditions

- El cliente dispone de una sesión autenticada vinculada a su identificador de titular y opera exclusivamente sobre los centros de estética asociados a su cuenta.
- Tras ejecutar el cierre de sesión, la sesión queda revocada en el servidor y la interfaz vuelve de forma obligatoria a la pantalla de inicio de sesión sin exponer datos del centro previo.

## 6. Revision History

| Version | Date | Author | Description |
| :--- | :--- | :--- | :--- |
| 1.0.0 | 2026-10-09 | agent-product-analyst | Definición del caso de uso de registro de cuenta, inicio de sesión obligatorio y alta de centros propios. |
| 1.1.0 | 2026-10-09 | agent-product-analyst | Especificación explícita del flujo y postcondición de cierre de sesión (logout) con retorno a la pantalla de login. |
