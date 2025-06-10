using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class MeterType
{
    public int MeterTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
}
