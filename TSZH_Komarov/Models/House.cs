using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class House
{
    public int HouseId { get; set; }

    public int TszhId { get; set; }

    public int Number { get; set; }

    public string Address { get; set; } = null!;

    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();

    public virtual Tszh Tszh { get; set; } = null!;
}
