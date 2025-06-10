using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class VoteType
{
    public int VoteTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Poll> Polls { get; set; } = new List<Poll>();
}
