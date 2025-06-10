using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class PollOption
{
    public int PollOptionId { get; set; }

    public int PollId { get; set; }

    public string OptionText { get; set; } = null!;

    public virtual Poll Poll { get; set; } = null!;

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
