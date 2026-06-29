using System;
using System.Collections.Generic;

namespace EcoCycleCore.Models;

public partial class Canje
{
    public int CanjeId { get; set; }

    public int CuponId { get; set; }

    public int UsuarioId { get; set; }

    public string CodigoDigital { get; set; } = null!;

    public DateTime? FechaCanje { get; set; }

    public virtual Cupone Cupon { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
