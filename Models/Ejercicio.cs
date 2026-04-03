using System;
using System.Collections.Generic;

namespace GymAPI.Models;

public partial class Ejercicio
{
    public int EjercicioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? GrupoMuscular { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<RutinaEjercicio> RutinaEjercicios { get; set; } = new List<RutinaEjercicio>();
}
