using System;
using System.Collections.Generic;

namespace GymAPI.Models;

public partial class Rutina
{
    public int RutinaId { get; set; }

    public int SocioId { get; set; }

    public int? EntrenadorId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Objetivo { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public bool Activa { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Entrenadore? Entrenador { get; set; }

    public virtual ICollection<RutinaEjercicio> RutinaEjercicios { get; set; } = new List<RutinaEjercicio>();

    public virtual Socio Socio { get; set; } = null!;
}
