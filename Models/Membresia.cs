using System;
using System.Collections.Generic;

namespace GymAPI.Models;

public partial class Membresia
{
    public int MembresiaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int DuracionDias { get; set; }

    public decimal Precio { get; set; }

    public bool EsRenovable { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<SocioMembresium> SocioMembresia { get; set; } = new List<SocioMembresium>();
}
