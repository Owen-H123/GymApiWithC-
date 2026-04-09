using System;
using System.Collections.Generic;
using GymAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Data;

public partial class GymDbContext : DbContext
{
    public GymDbContext()
    {
    }

    public GymDbContext(DbContextOptions<GymDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencia> Asistencias { get; set; }

    public virtual DbSet<Ejercicio> Ejercicios { get; set; }

    public virtual DbSet<Entrenadore> Entrenadores { get; set; }

    public virtual DbSet<Membresia> Membresias { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Rutina> Rutinas { get; set; }

    public virtual DbSet<RutinaEjercicio> RutinaEjercicios { get; set; }

    public virtual DbSet<Socio> Socios { get; set; }

    public virtual DbSet<SocioEntrenador> SocioEntrenadors { get; set; }

    public virtual DbSet<SocioMembresium> SocioMembresia { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VSocioUltimaMembresium> VSocioUltimaMembresia { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
     // Conexión manejada en Program.cs
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.HasKey(e => e.AsistenciaId).HasName("PK__Asistenc__72710FA50E2BED19");

            entity.HasIndex(e => new { e.SocioId, e.FechaHoraEntrada }, "IX_Asistencias_Socio_Fecha");

            entity.Property(e => e.FechaHoraEntrada).HasPrecision(0);
            entity.Property(e => e.FechaHoraSalida).HasPrecision(0);
            entity.Property(e => e.Observaciones).HasMaxLength(300);

            entity.HasOne(d => d.RegistradaPorUser).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.RegistradaPorUserId)
                .HasConstraintName("FK_Asistencias_UsuarioReg");

            entity.HasOne(d => d.Socio).WithMany(p => p.Asistencia)
                .HasForeignKey(d => d.SocioId)
                .HasConstraintName("FK_Asistencias_Socio");
        });

        modelBuilder.Entity<Ejercicio>(entity =>
        {
            entity.HasKey(e => e.EjercicioId).HasName("PK__Ejercici__81222641252A8E8E");

            entity.HasIndex(e => e.Nombre, "UQ_Ejercicios_Nombre").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(400);
            entity.Property(e => e.GrupoMuscular).HasMaxLength(60);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Ejercicios_IsActive");
            entity.Property(e => e.Nombre).HasMaxLength(120);
        });

        modelBuilder.Entity<Entrenadore>(entity =>
        {
            entity.HasKey(e => e.EntrenadorId).HasName("PK__Entrenad__D0EE856572F7515A");

            entity.HasIndex(e => e.UserId, "UQ__Entrenad__1788CC4D906BFE98").IsUnique();

            entity.Property(e => e.Certificaciones).HasMaxLength(250);
            entity.Property(e => e.Especialidad).HasMaxLength(120);
            entity.Property(e => e.FechaIngreso).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_Entrenadores_FechaIngreso");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Entrenadores_IsActive");

            entity.HasOne(d => d.User).WithOne(p => p.Entrenadore)
                .HasForeignKey<Entrenadore>(d => d.UserId)
                .HasConstraintName("FK_Entrenadores_User");
        });

        modelBuilder.Entity<Membresia>(entity =>
        {
            entity.HasKey(e => e.MembresiaId).HasName("PK__Membresi__5AE930976F48EC6D");

            entity.HasIndex(e => e.Nombre, "UQ_Membresias_Nombre").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Membresias_CreatedAt");
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.EsRenovable).HasDefaultValue(true, "DF_Membresias_EsRenovable");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Membresias_IsActive");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A0620BFDB");

            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.HasIndex(e => e.NormalizedName, "UQ_Roles_NormalizedName").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Roles_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Roles_IsActive");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NormalizedName).HasMaxLength(50);
        });

        modelBuilder.Entity<Rutina>(entity =>
        {
            entity.HasKey(e => e.RutinaId).HasName("PK__Rutinas__E76E167AD86ECF64");

            entity.HasIndex(e => new { e.SocioId, e.Activa }, "IX_Rutinas_Socio_Activa");

            entity.Property(e => e.Activa).HasDefaultValue(true, "DF_Rutinas_Activa");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Rutinas_CreatedAt");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_Rutinas_FechaInicio");
            entity.Property(e => e.Nombre).HasMaxLength(120);
            entity.Property(e => e.Objetivo).HasMaxLength(300);

            entity.HasOne(d => d.Entrenador).WithMany(p => p.Rutinas)
                .HasForeignKey(d => d.EntrenadorId)
                .HasConstraintName("FK_Rutinas_Entr");

            entity.HasOne(d => d.Socio).WithMany(p => p.Rutinas)
                .HasForeignKey(d => d.SocioId)
                .HasConstraintName("FK_Rutinas_Socio");
        });

        modelBuilder.Entity<RutinaEjercicio>(entity =>
        {
            entity.HasKey(e => new { e.RutinaId, e.EjercicioId }).HasName("PK__RutinaEj__EF7C341EB8183C79");

            entity.HasIndex(e => new { e.RutinaId, e.Orden }, "IX_RutinaEjercicios_Rutina_Orden");

            entity.Property(e => e.Notas).HasMaxLength(250);
            entity.Property(e => e.PesoObjetivoKg).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.Ejercicio).WithMany(p => p.RutinaEjercicios)
                .HasForeignKey(d => d.EjercicioId)
                .HasConstraintName("FK_RutinaEj_Ejercicio");

            entity.HasOne(d => d.Rutina).WithMany(p => p.RutinaEjercicios)
                .HasForeignKey(d => d.RutinaId)
                .HasConstraintName("FK_RutinaEj_Rutina");
        });

        modelBuilder.Entity<Socio>(entity =>
        {
            entity.HasKey(e => e.SocioId).HasName("PK__Socios__165D08BAEE91EAA6");

            entity.HasIndex(e => e.UserId, "UQ__Socios__1788CC4DC1C7871F").IsUnique();

            entity.Property(e => e.AlturaCm).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EmergenciaNombre).HasMaxLength(120);
            entity.Property(e => e.EmergenciaTelefono).HasMaxLength(25);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(CONVERT([date],sysdatetime()))", "DF_Socios_FechaRegistro");
            entity.Property(e => e.Genero)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Socios_IsActive");
            entity.Property(e => e.PesoKg).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.User).WithOne(p => p.Socio)
                .HasForeignKey<Socio>(d => d.UserId)
                .HasConstraintName("FK_Socios_User");
        });

        modelBuilder.Entity<SocioEntrenador>(entity =>
        {
            entity.HasKey(e => new { e.SocioId, e.EntrenadorId }).HasName("PK__SocioEnt__5B53E0ECC991A6CD");

            entity.ToTable("SocioEntrenador");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaAsignacion).HasDefaultValueSql("(CONVERT([date],sysdatetime()))");

            entity.HasOne(d => d.Entrenador).WithMany(p => p.SocioEntrenadors)
                .HasForeignKey(d => d.EntrenadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SocioEntrenador_Entr");

            entity.HasOne(d => d.Socio).WithMany(p => p.SocioEntrenadors)
                .HasForeignKey(d => d.SocioId)
                .HasConstraintName("FK_SocioEntrenador_Socio");
        });

        modelBuilder.Entity<SocioMembresium>(entity =>
        {
            entity.HasKey(e => e.SocioMembresiaId).HasName("PK__SocioMem__F467686A1AE6F195");

            entity.HasIndex(e => new { e.Estado, e.FechaFin }, "IX_SocioMembresia_Estado_FechaFin");

            entity.HasIndex(e => e.SocioId, "IX_SocioMembresia_Socio");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_SocioMembresia_CreatedAt");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MontoPagado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Notas).HasMaxLength(300);

            entity.HasOne(d => d.Membresia).WithMany(p => p.SocioMembresia)
                .HasForeignKey(d => d.MembresiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SocioMembresia_Membresia");

            entity.HasOne(d => d.Socio).WithMany(p => p.SocioMembresia)
                .HasForeignKey(d => d.SocioId)
                .HasConstraintName("FK_SocioMembresia_Socio");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C91A326D5");

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.HasIndex(e => e.NormalizedEmail, "UQ_Users_NormalizedEmail").IsUnique();

            entity.HasIndex(e => e.NormalizedUserName, "UQ_Users_NormalizedUserName").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ_Users_UserName").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Users_CreatedAt");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Users_IsActive");
            entity.Property(e => e.LastLoginAt).HasPrecision(0);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.PhoneNumber).HasMaxLength(25);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760ADC0C40D69");

            entity.HasIndex(e => e.RoleId, "IX_UserRoles_RoleId");

            entity.Property(e => e.AssignedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_UserRoles_AssignedAt");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_UserRoles_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRoles_User");
        });

        modelBuilder.Entity<VSocioUltimaMembresium>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vSocioUltimaMembresia");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Membresia).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
