using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class Apartment
{
    public int ApartmentId { get; set; }

    public int HouseId { get; set; }

    public int? UserId { get; set; }

    public string Number { get; set; } = null!;

    public string PersonalAccount { get; set; } = null!;

    public double Meters { get; set; }

    public virtual House House { get; set; } = null!;

    public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();

    public virtual AppUser? User { get; set; }
}
