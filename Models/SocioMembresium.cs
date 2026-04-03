using System;
using System.Collections.Generic;

namespace GymAPI.Models;

public partial class SocioMembresium
{
    public int SocioMembresiaId { get; set; }

    public int SocioId { get; set; }

    public int MembresiaId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Estado { get; set; } = null!;

    public decimal MontoPagado { get; set; }

    public string? Notas { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Membresia Membresia { get; set; } = null!;

    public virtual Socio Socio { get; set; } = null!;
}
