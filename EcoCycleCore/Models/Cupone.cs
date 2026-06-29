using System;
using System.Collections.Generic;

namespace EcoCycleCore.Models;

public partial class Cupone
{
    public int CuponId { get; set; }

    public string TiendaNombre { get; set; } = null!;

    public string DescBeneficio { get; set; } = null!;

    public int CostPuntos { get; set; }

    public int Stock { get; set; }

    public virtual ICollection<Canje> Canjes { get; set; } = new List<Canje>();
}
