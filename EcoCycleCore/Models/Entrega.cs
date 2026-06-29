using System;
using System.Collections.Generic;

namespace EcoCycleCore.Models;

public partial class Entrega
{
    public int EntregaId { get; set; }

    public int PublicacionId { get; set; }

    public int CiudadanoId { get; set; }

    public int CentroId { get; set; }

    public decimal PesoReal { get; set; }

    public int PuntosOtorgados { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public virtual Usuario Ciudadano { get; set; } = null!;

    public virtual Publicacione Publicacion { get; set; } = null!;
}
