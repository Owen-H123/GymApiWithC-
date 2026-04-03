using System;
using System.Collections.Generic;

namespace GymAPI.Models;

public partial class Entrenadore
{
    public int EntrenadorId { get; set; }

    public int UserId { get; set; }

    public string? Especialidad { get; set; }

    public string? Certificaciones { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Rutina> Rutinas { get; set; } = new List<Rutina>();

    public virtual ICollection<SocioEntrenador> SocioEntrenadors { get; set; } = new List<SocioEntrenador>();

    public virtual User User { get; set; } = null!;
}
