using System;
using System.Collections.Generic;

namespace EcoCycleCore.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public string TipoUsuario { get; set; } = null!;

    public int? PuntosAcumulacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Canje> Canjes { get; set; } = new List<Canje>();

    public virtual ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    public virtual ICollection<Publicacione> Publicaciones { get; set; } = new List<Publicacione>();
}
