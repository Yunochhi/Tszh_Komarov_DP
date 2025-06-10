using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class Tszh
{
    public int TszhId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public virtual ICollection<House> Houses { get; set; } = new List<House>();
}
