using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class ServiceType
{
    public int ServiceTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
