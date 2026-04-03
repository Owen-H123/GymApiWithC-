using System;
using System.Collections.Generic;

namespace GymAPI.Models;

public partial class Asistencia
{
    public int AsistenciaId { get; set; }

    public int SocioId { get; set; }

    public DateTime FechaHoraEntrada { get; set; }

    public DateTime? FechaHoraSalida { get; set; }

    public string? Observaciones { get; set; }

    public int? RegistradaPorUserId { get; set; }

    public virtual User? RegistradaPorUser { get; set; }

    public virtual Socio Socio { get; set; } = null!;
}
