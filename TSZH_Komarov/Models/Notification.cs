using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Topic { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int TszhId { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
