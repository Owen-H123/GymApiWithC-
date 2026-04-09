# Documentación Técnica: Sistema de Gestión de Gimnasio (GymAPI)

Esta es la documentación técnica y arquitectónica del proyecto backend desarrollado en **.NET 8** utilizando **Entity Framework Core**, **SQL Server** en **Docker**, y seguridad gestionada a través de **JSON Web Tokens (JWT) + BCrypt**.

---

## 1. Arquitectura del Proyecto

El sistema está estructurado bajo el patrón de **API RESTful** y diseñado monolíticamente para microservicios. Adopta una aproximación **Database-First** donde el modelo base de datos (contenido en `script-bdgym.sql`) es la fuente principal de verdad de la cual se extrapolado los Modelos (`DbContext` y clases C# correspondientes).

### Capas Principales:
* **Controllers** (`/Controllers`): Actúan como los "Endpoints" o puntos de entrada para las peticiones HTTP. Cada controlador (Ej. `AuthController`, `EntrenadoresController`) define rutas de red y métodos (GET, POST).
* **Data** (`/Data`): Contiene `GymDbContext.cs`, la capa que traduce las peticiones de C# a comandos SQL usando **Entity Framework Core**.
* **Models** (`/Models`): Las entidades de negocio fuertemente tipadas (User, Socio, Entrenadore, Rutina, etc.).

---

## 2. Pila Tecnológica (Tech Stack)

* **Backend:** C# / .NET 8 (Web API)
* **ORM:** Entity Framework Core
* **Base de Datos:** Microsoft SQL Server
* **Autenticación:** JWT (Bearer Tokens)
* **Criptografía:** Librería `BCrypt.Net` para el hash de contraseñas.
* **Infraestructura:** Docker Compose (Despliegue local de Base de Datos).

---

## 3. Seguridad y Control de Acceso

La API utiliza un modelo de seguridad estricto que asegura que la aplicación consumidora (como un Frontend web o aplicación móvil) solo acceda a lo que le corresponde.

### 3.1. Proceso de Autenticación
1. El usuario envía credenciales (`Email` y `Password`) al endpoint `/api/auth/login`.
2. El sistema busca el `User` normalizando el email a mayúsculas.
3. El sistema valida la contraseña utilizando el método `BCrypt.Verify()` contrastándola con el `PasswordHash` de la base de datos.
4. Si es válida, el `AuthController` carga del archivo local (`appsettings.json`) el `SecretKey` temporal.
5. Se genera un **Json Web Token (JWT)** inyectando el Claim (Identificador) del rol del usuario.

### 3.2. Autorización por Roles
Las rutas están bloqueadas a través de la etiqueta `[Authorize(Roles = "...")]`.
* `ADMIN`: Acceso global al sistema y gestión estructural.
* `ENTRENADOR`: Permisos limitados a lectura y gestión de rutinas.
* `SOCIO`: Acceso de sólo lectura a registros que le pertenecen a sí mismo.

### 3.3. Políticas de CORS (Seguridad Transversal)
El Backend cuenta con una política global (`AllowFrontend`) configurada en `Program.cs` que permite que clientes web de otros puertos interactúen fluidamente sin ser bloqueados por los navegadores modernos.

---

## 4. Base de Datos Crítica

El corazón de los datos vive en una instancia Dockerizada en el puerto `1433`.

### 4.1. Entidades Principales
* **Users & Roles**: Los `Users` actúan como cuentas del sistema. Su información clínica/física se expande en las tablas `Socios` o profesional en `Entrenadores` de manera polimórfica (1:1). 
* **Catálogos Clave**: `Membresías`, `Ejercicios`, `Roles`.
* **Transaccionales**: `Asistencias` (Check-ins), `SocioMembresia` (Renovaciones), `Rutinas` (Intersección dinámica con Ejercicios y Socios).

**Nota Técnica de Inicialización**: 
La base de datos es inicializada inyectando el script en el contenedor:
```bash
docker exec -i gym_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Root1234 -C -i - < script-bdgym.sql
```

---

## 5. Mejores Prácticas Recientes (Refactor Updates)

* **Separación de Responsabilidades Config/Código**:
  La cadena de conexión (`connectionString`) fue removida totalmente del `DbContext`. La inyección de dependencias (`DI`) desde `Program.cs` y su anclaje en `appsettings.json` garantizan la seguridad de credenciales en producción.
* **Tiempos Base JWT Dinámicos**:
  La expiración del token (`ExpirationHours`) se ajustó dinámicamente para ser gobernada por el entorno centralizado, evitando los *Hardcodes*.
* **Control de Rutas API**:
  Modificaciones realizadas asumen una arquitectura completamente Desacoplada ("Headless").
