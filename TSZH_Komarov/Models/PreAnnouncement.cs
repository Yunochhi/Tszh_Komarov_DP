using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class PreAnnouncement
{
    public int PreAnnouncementId { get; set; }

    public int TszhId { get; set; }

    public string Topic { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Priority { get; set; }

    public DateTime Date { get; set; }
}
