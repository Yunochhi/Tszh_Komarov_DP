using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class MeterReading
{
    public int MeterReadingsId { get; set; }

    public int ApartamentId { get; set; }

    public int MeterTypeId { get; set; }

    public double Value { get; set; }

    public DateTime ReadingDate { get; set; }

    public virtual Apartment Apartament { get; set; } = null!;

    public virtual MeterType MeterType { get; set; } = null!;
}
