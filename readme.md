# GymAPI — Sistema de Gestión de Gimnasio

API RESTful desarrollada en **.NET Core** con autenticación **JWT** y base de datos **SQL Server**, todo containerizado con **Docker**.

---

##  Requisitos Previos

Instala las siguientes herramientas antes de comenzar:

| Herramienta | Descarga |
|-------------|----------|
| Docker Desktop | https://www.docker.com/products/docker-desktop |
| .NET 8 SDK | https://dotnet.microsoft.com/download/dotnet/8 |
| Git | https://git-scm.com |
| Postman | https://www.postman.com/downloads |

---

## Estructura del Proyecto

GymAPI/
├── Controllers/        → Endpoints de la API
├── Data/               → DbContext (Entity Framework)
├── DTOs/               → Objetos de transferencia
├── Models/             → Entidades generadas con Scaffold
├── Services/           → Lógica de negocio
├── script-bdgym.sql    → Script de base de datos
├── docker-compose.yml  → Configuración Docker
├── Dockerfile          → Imagen de la API
└── appsettings.json    → Configuración de la aplicación

---

##  Instalación Rápida

### 1. Clonar y Entrar
```bash
git clone https://github.com/Owen-H123/GymApiWithC-.git
cd GymApiWithC-
```

### 2. Levantar SQL Server
```bash
docker compose up -d
```
> [!TIP]
> Espera a que el contenedor esté **Healthy** (puedes verlo con `docker ps`).

### 3. Inicializar Base de Datos
Ejecuta este comando para crear las tablas y cargar los datos de prueba:
```bash
docker exec -i gym_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Root1234" -C < script-bdgym.sql
```

### 4. Ejecutar API
```bash
dotnet restore
dotnet run
```
La API estará en: `http://localhost:5252`

---

## Credenciales de Prueba
Usa estos usuarios para obtener tu token en `POST /api/auth/login`:

| Usuario | Email | Password | Rol |
|---------|-------|----------|-----|
| **Admin** | `admin@gym.local` | `Admin123!` | `ADMIN` |
| **Entrenador** | `entrenador@gym.local` | `Trainer123!` | `ENTRENADOR` |
| **Socio** | `socio@gym.local` | `Socio123!` | `SOCIO` |

---

## 🛠️ Endpoints Principales
| Método | Ruta | Descripción | Requiere |
|--------|------|-------------|----------|
| POST | `/api/auth/login` | Obtener Token JWT | Público |
| GET | `/api/socios` | Listar Socios | Token ADMIN |
| GET | `/api/asistencias` | Ver Asistencias | Token ADMIN |
| POST | `/api/rutinas` | Crear Rutina | Token ENTRENADOR |

---

##  Configuración

El archivo `appsettings.json` contiene la configuración principal:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=GimnasioDB;User Id=sa;Password=Root1234;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "GymSecretKey2024!SuperSegura#JWT",
    "Issuer": "GymAPI",
    "Audience": "GymClients",
    "ExpirationHours": 8
  }
}
```

---

##  Autenticación JWT

La API usa JWT para autenticación sin estado. Los roles disponibles son:

| Rol | Permisos |
|-----|----------|
| `ADMIN` | Gestión completa de socios, entrenadores y membresías |
| `ENTRENADOR` | Registrar asistencias, consultar socios asignados |
| `SOCIO` | Ver su historial de asistencias y plan de entrenamiento |

### Obtener Token

**POST** `http://localhost:5252/api/auth/login`
```json
{
  "email": "admin@gym.local",
  "password": "Admin123!"
}
```

Usar el token en cada request:
Authorization: Bearer <token>

---

##  Endpoints

### Auth
| Método | Ruta | Descripción | Rol |
|--------|------|-------------|-----|
| POST | `/api/auth/login` | Iniciar sesión | Público |

### Socios
| Método | Ruta | Descripción | Rol |
|--------|------|-------------|-----|
| GET | `/api/socios` | Listar socios | ADMIN |
| GET | `/api/socios/{id}` | Ver socio | ADMIN, ENTRENADOR, SOCIO |
| POST | `/api/socios` | Crear socio | ADMIN |
| DELETE | `/api/socios/{id}` | Desactivar socio | ADMIN |

### Entrenadores
| Método | Ruta | Descripción | Rol |
|--------|------|-------------|-----|
| GET | `/api/entrenadores` | Listar entrenadores | ADMIN, SOCIO |
| GET | `/api/entrenadores/{id}` | Ver entrenador | ADMIN, ENTRENADOR, SOCIO |
| POST | `/api/entrenadores` | Crear entrenador | ADMIN |
| DELETE | `/api/entrenadores/{id}` | Desactivar entrenador | ADMIN |

### Asistencias
| Método | Ruta | Descripción | Rol |
|--------|------|-------------|-----|
| GET | `/api/asistencias` | Listar asistencias | ADMIN |
| GET | `/api/asistencias/socio/{id}` | Historial del socio | ADMIN, ENTRENADOR, SOCIO |
| POST | `/api/asistencias` | Registrar asistencia | ADMIN, ENTRENADOR |
| PUT | `/api/asistencias/{id}/salida` | Registrar salida | ADMIN, ENTRENADOR |

### Rutinas
| Método | Ruta | Descripción | Rol |
|--------|------|-------------|-----|
| GET | `/api/rutinas/socio/{id}` | Ver rutinas del socio | ADMIN, ENTRENADOR, SOCIO |
| POST | `/api/rutinas` | Crear rutina | ADMIN, ENTRENADOR |
| DELETE | `/api/rutinas/{id}` | Desactivar rutina | ADMIN |

### Membresías
| Método | Ruta | Descripción | Rol |
|--------|------|-------------|-----|
| GET | `/api/membresias` | Listar membresías | ADMIN |
| POST | `/api/membresias` | Crear nueva membresía | ADMIN |
| POST | `/api/membresias/asignar` | Asignar membresía a socio | ADMIN |

---

## 🧪 Pruebas con Postman

> **Nota:** En la raíz del proyecto se incluye el archivo `GymAPI_Postman_Collection.json` listo para ser importado en Postman. Esta colección contiene todos los endpoints y tokens ya configurados automáticamente tras el Login.

### Flujo básico de pruebas:

1. **Login** → `POST /api/auth/login` → obtener token
2. **Crear socio** → `POST /api/socios` con token ADMIN
3. **Crear entrenador** → `POST /api/entrenadores` con token ADMIN
4. **Registrar asistencia** → `POST /api/asistencias` con token ADMIN o ENTRENADOR
5. **Ver historial** → `GET /api/asistencias/socio/1` con token
6. **Ver rutinas** → `GET /api/rutinas/socio/1` con token

---

## Base de Datos

### Diagrama de tablas principales:
Users ──── UserRoles ──── Roles
│
├── Socios ──── Asistencias
│      └────── Rutinas ──── RutinaEjercicios ──── Ejercicios
│      └────── SocioMembresia ──── Membresias
│      └────── SocioEntrenador
│
└── Entrenadores

---

##  Equipo de Desarrollo

- Fabricio
- Bionda
- Eduardo
- Owen
- Bryan
