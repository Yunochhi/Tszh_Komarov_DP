using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class ServiceRequest
{
    public int ServiceRequestId { get; set; }

    public int ServiceTypeId { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? Note { get; set; }

    public string? AdminComment { get; set; }

    public int ApartmentId { get; set; }

    public virtual ServiceType ServiceType { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
