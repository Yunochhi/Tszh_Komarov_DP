using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class Poll
{
    public int PollId { get; set; }

    public int VoteTypeId { get; set; }

    public int TszhId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime EndDate { get; set; }

    public virtual ICollection<PollOption> PollOptions { get; set; } = new List<PollOption>();

    public virtual VoteType VoteType { get; set; } = null!;
}
