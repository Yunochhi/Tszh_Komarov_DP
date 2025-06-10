using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class Vote
{
    public int VotesId { get; set; }

    public int PollOptionId { get; set; }

    public int UserId { get; set; }

    public virtual PollOption PollOption { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
