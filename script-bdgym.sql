/* =========================================================
   GIMNASIO – Base de datos (SQL Server)
   ========================================================= */

-- 0) Crear base (opcional)
IF DB_ID(N'GimnasioDB') IS NULL
BEGIN
    CREATE DATABASE GimnasioDB;
END
GO

USE GimnasioDB;
GO

/* =========================================================
   1) ESQUEMA (dbo por defecto)
   ========================================================= */

-- Sugerencia: si quisieras separar seguridad/dominio:
-- CREATE SCHEMA sec;  -- y mover Users/Roles/UserRoles a sec.
-- Para este ejemplo usamos dbo por simplicidad.

/* =========================================================
   2) TABLAS DE SEGURIDAD
   ========================================================= */

-- Roles del sistema
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL DROP TABLE dbo.Roles;
GO
CREATE TABLE dbo.Roles (
    RoleId           INT IDENTITY(1,1) PRIMARY KEY,
    Name             NVARCHAR(50) NOT NULL,
    NormalizedName   NVARCHAR(50) NOT NULL,
    IsActive         BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT(1),
    CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT(SYSDATETIME()),
    CONSTRAINT UQ_Roles_Name UNIQUE (Name),
    CONSTRAINT UQ_Roles_NormalizedName UNIQUE (NormalizedName)
);

-- Usuarios (cuenta de autenticación)
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO
CREATE TABLE dbo.Users (
    UserId             INT IDENTITY(1,1) PRIMARY KEY,
    UserName           NVARCHAR(100) NOT NULL,
    NormalizedUserName NVARCHAR(100) NOT NULL,
    Email              NVARCHAR(256) NOT NULL,
    NormalizedEmail    NVARCHAR(256) NOT NULL,
    PasswordHash       NVARCHAR(512) NOT NULL, -- hash (no texto plano)
    PhoneNumber        NVARCHAR(25)  NULL,
    IsActive           BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
    LastLoginAt        DATETIME2(0) NULL,
    CreatedAt          DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT(SYSDATETIME()),
    UpdatedAt          DATETIME2(0) NULL,
    CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
    CONSTRAINT UQ_Users_NormalizedUserName UNIQUE (NormalizedUserName),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT UQ_Users_NormalizedEmail UNIQUE (NormalizedEmail)
);

-- Relación N:N entre Users y Roles
IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NOT NULL DROP TABLE dbo.UserRoles;
GO
CREATE TABLE dbo.UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME2(0) NOT NULL CONSTRAINT DF_UserRoles_AssignedAt DEFAULT(SYSDATETIME()),
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId) ON DELETE CASCADE
);

-- Índices útiles
CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles(RoleId);
GO

/* =========================================================
   3) TABLAS DE DOMINIO
   ========================================================= */

-- Entrenadores (extensión de Users)
IF OBJECT_ID(N'dbo.Entrenadores', N'U') IS NOT NULL DROP TABLE dbo.Entrenadores;
GO
CREATE TABLE dbo.Entrenadores (
    EntrenadorId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT NOT NULL UNIQUE, -- 1:1 con Users
    Especialidad   NVARCHAR(120) NULL,
    Certificaciones NVARCHAR(250) NULL,
    FechaIngreso   DATE NOT NULL CONSTRAINT DF_Entrenadores_FechaIngreso DEFAULT(CONVERT(date, SYSDATETIME())),
    IsActive       BIT NOT NULL CONSTRAINT DF_Entrenadores_IsActive DEFAULT(1),
    CONSTRAINT FK_Entrenadores_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
);

-- Socios (extensión de Users)
IF OBJECT_ID(N'dbo.Socios', N'U') IS NOT NULL DROP TABLE dbo.Socios;
GO
CREATE TABLE dbo.Socios (
    SocioId              INT IDENTITY(1,1) PRIMARY KEY,
    UserId               INT NOT NULL UNIQUE, -- 1:1 con Users
    FechaNacimiento      DATE NULL,
    Genero               CHAR(1) NULL CHECK (Genero IN ('M','F','O') OR Genero IS NULL),
    AlturaCm             DECIMAL(5,2) NULL CHECK (AlturaCm >= 0),
    PesoKg               DECIMAL(6,2) NULL CHECK (PesoKg >= 0),
    EmergenciaNombre     NVARCHAR(120) NULL,
    EmergenciaTelefono   NVARCHAR(25)  NULL,
    FechaRegistro        DATE NOT NULL CONSTRAINT DF_Socios_FechaRegistro DEFAULT(CONVERT(date, SYSDATETIME())),
    IsActive             BIT NOT NULL CONSTRAINT DF_Socios_IsActive DEFAULT(1),
    CONSTRAINT FK_Socios_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
);

-- Asignación N:N Socio-Entrenador
IF OBJECT_ID(N'dbo.SocioEntrenador', N'U') IS NOT NULL DROP TABLE dbo.SocioEntrenador;
GO
CREATE TABLE dbo.SocioEntrenador (
    SocioId          INT NOT NULL,
    EntrenadorId     INT NOT NULL,
    FechaAsignacion  DATE NOT NULL CONSTRAINT DF_SocioEntrenador_FechaAsign DEFAULT(CONVERT(date, SYSDATETIME())),
    Activo           BIT NOT NULL CONSTRAINT DF_SocioEntrenador_Activo DEFAULT(1),
    PRIMARY KEY (SocioId, EntrenadorId),
    CONSTRAINT FK_SocioEntrenador_Socio FOREIGN KEY (SocioId) REFERENCES dbo.Socios(SocioId) ON DELETE CASCADE,
    CONSTRAINT FK_SocioEntrenador_Entr FOREIGN KEY (EntrenadorId) REFERENCES dbo.Entrenadores(EntrenadorId) ON DELETE CASCADE
);

-- Catálogo de Membresías
IF OBJECT_ID(N'dbo.Membresias', N'U') IS NOT NULL DROP TABLE dbo.Membresias;
GO
CREATE TABLE dbo.Membresias (
    MembresiaId   INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Descripcion   NVARCHAR(300) NULL,
    DuracionDias  INT NOT NULL CHECK (DuracionDias > 0),
    Precio        DECIMAL(10,2) NOT NULL CHECK (Precio >= 0),
    EsRenovable   BIT NOT NULL CONSTRAINT DF_Membresias_EsRenovable DEFAULT(1),
    IsActive      BIT NOT NULL CONSTRAINT DF_Membresias_IsActive DEFAULT(1),
    CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Membresias_CreatedAt DEFAULT(SYSDATETIME()),
    CONSTRAINT UQ_Membresias_Nombre UNIQUE (Nombre)
);

-- Membresías por socio (histórico)
IF OBJECT_ID(N'dbo.SocioMembresia', N'U') IS NOT NULL DROP TABLE dbo.SocioMembresia;
GO
CREATE TABLE dbo.SocioMembresia (
    SocioMembresiaId INT IDENTITY(1,1) PRIMARY KEY,
    SocioId          INT NOT NULL,
    MembresiaId      INT NOT NULL,
    FechaInicio      DATE NOT NULL,
    FechaFin         DATE NOT NULL,
    Estado           VARCHAR(20) NOT NULL
                     CHECK (Estado IN ('ACTIVA','VENCIDA','PAUSADA','CANCELADA')),
    MontoPagado      DECIMAL(10,2) NOT NULL CHECK (MontoPagado >= 0),
    Notas            NVARCHAR(300) NULL,
    CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_SocioMembresia_CreatedAt DEFAULT(SYSDATETIME()),
    CONSTRAINT FK_SocioMembresia_Socio FOREIGN KEY (SocioId) REFERENCES dbo.Socios(SocioId) ON DELETE CASCADE,
    CONSTRAINT FK_SocioMembresia_Membresia FOREIGN KEY (MembresiaId) REFERENCES dbo.Membresias(MembresiaId)
);

-- Índices de consulta frecuentes
CREATE INDEX IX_SocioMembresia_Socio ON dbo.SocioMembresia(SocioId);
CREATE INDEX IX_SocioMembresia_Estado_FechaFin ON dbo.SocioMembresia(Estado, FechaFin);

-- Asistencias (check-in / check-out)
IF OBJECT_ID(N'dbo.Asistencias', N'U') IS NOT NULL DROP TABLE dbo.Asistencias;
GO
CREATE TABLE dbo.Asistencias (
    AsistenciaId        INT IDENTITY(1,1) PRIMARY KEY,
    SocioId             INT NOT NULL,
    FechaHoraEntrada    DATETIME2(0) NOT NULL,
    FechaHoraSalida     DATETIME2(0) NULL,
    Observaciones       NVARCHAR(300) NULL,
    RegistradaPorUserId INT NULL, -- quien registró (ej. recepcionista/admin)
    CONSTRAINT FK_Asistencias_Socio FOREIGN KEY (SocioId) REFERENCES dbo.Socios(SocioId) ON DELETE CASCADE,
    CONSTRAINT FK_Asistencias_UsuarioReg FOREIGN KEY (RegistradaPorUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Asistencias_Rango CHECK (FechaHoraSalida IS NULL OR FechaHoraSalida >= FechaHoraEntrada)
);

CREATE INDEX IX_Asistencias_Socio_Fecha ON dbo.Asistencias(SocioId, FechaHoraEntrada);

-- Catálogo de ejercicios
IF OBJECT_ID(N'dbo.Ejercicios', N'U') IS NOT NULL DROP TABLE dbo.Ejercicios;
GO
CREATE TABLE dbo.Ejercicios (
    EjercicioId   INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(120) NOT NULL,
    Descripcion   NVARCHAR(400) NULL,
    GrupoMuscular NVARCHAR(60)  NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Ejercicios_IsActive DEFAULT(1),
    CONSTRAINT UQ_Ejercicios_Nombre UNIQUE (Nombre)
);

-- Rutinas de entrenamiento
IF OBJECT_ID(N'dbo.Rutinas', N'U') IS NOT NULL DROP TABLE dbo.Rutinas;
GO
CREATE TABLE dbo.Rutinas (
    RutinaId      INT IDENTITY(1,1) PRIMARY KEY,
    SocioId       INT NOT NULL,
    EntrenadorId  INT NULL,
    Nombre        NVARCHAR(120) NOT NULL,
    Objetivo      NVARCHAR(300) NULL,
    FechaInicio   DATE NOT NULL CONSTRAINT DF_Rutinas_FechaInicio DEFAULT(CONVERT(date, SYSDATETIME())),
    FechaFin      DATE NULL,
    Activa        BIT NOT NULL CONSTRAINT DF_Rutinas_Activa DEFAULT(1),
    CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Rutinas_CreatedAt DEFAULT(SYSDATETIME()),
    CONSTRAINT FK_Rutinas_Socio FOREIGN KEY (SocioId) REFERENCES dbo.Socios(SocioId) ON DELETE CASCADE,
    CONSTRAINT FK_Rutinas_Entr FOREIGN KEY (EntrenadorId) REFERENCES dbo.Entrenadores(EntrenadorId)
);

CREATE INDEX IX_Rutinas_Socio_Activa ON dbo.Rutinas(SocioId, Activa);

-- Detalle de ejercicios por rutina (N:N con atributos)
IF OBJECT_ID(N'dbo.RutinaEjercicios', N'U') IS NOT NULL DROP TABLE dbo.RutinaEjercicios;
GO
CREATE TABLE dbo.RutinaEjercicios (
    RutinaId          INT NOT NULL,
    EjercicioId       INT NOT NULL,
    Orden             INT NOT NULL CHECK (Orden > 0),
    Series            INT NULL CHECK (Series IS NULL OR Series > 0),
    Repeticiones      INT NULL CHECK (Repeticiones IS NULL OR Repeticiones > 0),
    PesoObjetivoKg    DECIMAL(6,2) NULL CHECK (PesoObjetivoKg IS NULL OR PesoObjetivoKg >= 0),
    DuracionSegundos  INT NULL CHECK (DuracionSegundos IS NULL OR DuracionSegundos >= 0),
    DescansoSegundos  INT NULL CHECK (DescansoSegundos IS NULL OR DescansoSegundos >= 0),
    Notas             NVARCHAR(250) NULL,
    PRIMARY KEY (RutinaId, EjercicioId),
    CONSTRAINT FK_RutinaEj_Rutina FOREIGN KEY (RutinaId) REFERENCES dbo.Rutinas(RutinaId) ON DELETE CASCADE,
    CONSTRAINT FK_RutinaEj_Ejercicio FOREIGN KEY (EjercicioId) REFERENCES dbo.Ejercicios(EjercicioId) ON DELETE CASCADE
);

CREATE INDEX IX_RutinaEjercicios_Rutina_Orden ON dbo.RutinaEjercicios(RutinaId, Orden);
GO

/* =========================================================
   4) SEEDS BÁSICOS
   ========================================================= */

-- Roles base
INSERT INTO dbo.Roles (Name, NormalizedName) VALUES
 (N'ADMIN',       N'ADMIN'),
 (N'ENTRENADOR',  N'ENTRENADOR'),
 (N'SOCIO',       N'SOCIO');
GO

-- (Opcional) Usuarios de ejemplo (password hash simulado)
-- En producción, este hash lo genera tu backend (PBKDF2/BCrypt/Argon2).
INSERT INTO dbo.Users (UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash, PhoneNumber)
VALUES
 (N'admin',      N'ADMIN',      N'admin@gym.local',      N'ADMIN@GYM.LOCAL',      N'**HASH**', N'+51 900000001'),
 (N'entrenador', N'ENTRENADOR', N'entrenador@gym.local', N'ENTRENADOR@GYM.LOCAL', N'**HASH**', N'+51 900000002'),
 (N'socio',      N'SOCIO',      N'socio@gym.local',      N'SOCIO@GYM.LOCAL',      N'**HASH**', N'+51 900000003');
GO

-- Asociar roles a usuarios demo
INSERT INTO dbo.UserRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM dbo.Users u
JOIN dbo.Roles r ON r.Name = N'ADMIN'
WHERE u.UserName = N'admin';

INSERT INTO dbo.UserRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM dbo.Users u
JOIN dbo.Roles r ON r.Name = N'ENTRENADOR'
WHERE u.UserName = N'entrenador';

INSERT INTO dbo.UserRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM dbo.Users u
JOIN dbo.Roles r ON r.Name = N'SOCIO'
WHERE u.UserName = N'socio';
GO

-- Crear perfiles extendidos
INSERT INTO dbo.Entrenadores (UserId, Especialidad, Certificaciones)
SELECT UserId, N'Fuerza e Hiperplasia', N'NSCA-CPT'
FROM dbo.Users WHERE UserName = N'entrenador';

INSERT INTO dbo.Socios (UserId, FechaNacimiento, Genero, AlturaCm, PesoKg, EmergenciaNombre, EmergenciaTelefono)
SELECT UserId, '1995-01-22', 'M', 175.0, 72.5, N'Contacto Demo', N'+51 955555555'
FROM dbo.Users WHERE UserName = N'socio';
GO

-- Catálogo mínimo de membresías
INSERT INTO dbo.Membresias (Nombre, Descripcion, DuracionDias, Precio, EsRenovable)
VALUES
 (N'Basic',   N'Acceso a sala de máquinas',           30,  99.00, 1),
 (N'Plus',    N'Basic + clases grupales',              30, 149.00, 1),
 (N'Premium', N'Plus + zona peso libre + entrenador',  30, 199.00, 1);
GO

-- Asignar una membresía al socio demo
INSERT INTO dbo.SocioMembresia (SocioId, MembresiaId, FechaInicio, FechaFin, Estado, MontoPagado, Notas)
SELECT s.SocioId, m.MembresiaId, CONVERT(date, SYSDATETIME()),
       DATEADD(DAY, m.DuracionDias, CONVERT(date, SYSDATETIME())),
       'ACTIVA', m.Precio, N'Alta inicial'
FROM dbo.Socios s
CROSS APPLY (SELECT TOP 1 MembresiaId, DuracionDias, Precio FROM dbo.Membresias WHERE Nombre = N'Plus') m
WHERE s.UserId = (SELECT UserId FROM dbo.Users WHERE UserName = N'socio');
GO

-- Ejercicios base
INSERT INTO dbo.Ejercicios (Nombre, Descripcion, GrupoMuscular) VALUES
 (N'Sentadilla', N'Sentadilla con barra', N'Piernas'),
 (N'Press Banca', N'Press en banco plano', N'Pectoral'),
 (N'Dominadas', N'Trácciones en barra', N'Espalda');
GO

-- Rutina de ejemplo para el socio, asignada por el entrenador
DECLARE @SocioId INT = (SELECT SocioId FROM dbo.Socios s JOIN dbo.Users u ON s.UserId = u.UserId WHERE u.UserName = N'socio');
DECLARE @EntrId INT = (SELECT EntrenadorId FROM dbo.Entrenadores e JOIN dbo.Users u ON e.UserId = u.UserId WHERE u.UserName = N'entrenador');

INSERT INTO dbo.Rutinas (SocioId, EntrenadorId, Nombre, Objetivo)
VALUES (@SocioId, @EntrId, N'Full Body Inicial', N'Adaptación neuromuscular');

DECLARE @RutinaId INT = SCOPE_IDENTITY();

INSERT INTO dbo.RutinaEjercicios (RutinaId, EjercicioId, Orden, Series, Repeticiones, PesoObjetivoKg, DescansoSegundos)
SELECT @RutinaId, EjercicioId, ROW_NUMBER() OVER(ORDER BY EjercicioId), 3, 10, NULL, 90
FROM dbo.Ejercicios;
GO

/* =========================================================
   5) VISTAS/CONSULTAS ÚTILES (opcionales)
   ========================================================= */

-- Última membresía de cada socio (por FechaFin)
IF OBJECT_ID(N'dbo.vSocioUltimaMembresia', N'V') IS NOT NULL DROP VIEW dbo.vSocioUltimaMembresia;
GO
CREATE VIEW dbo.vSocioUltimaMembresia AS
SELECT sm.SocioId, sm.MembresiaId, m.Nombre AS Membresia, sm.Estado, sm.FechaInicio, sm.FechaFin
FROM dbo.SocioMembresia sm
JOIN dbo.Membresias m ON m.MembresiaId = sm.MembresiaId
WHERE sm.FechaFin = (
    SELECT MAX(FechaFin) FROM dbo.SocioMembresia sm2 WHERE sm2.SocioId = sm.SocioId
);
GO
