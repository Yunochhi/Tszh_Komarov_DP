using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class TopicComment
{
    public int TopicCommentId { get; set; }

    public int TopicId { get; set; }

    public int UserId { get; set; }

    public string Description { get; set; } = null!;

    public int VotesCount { get; set; }

    public DateTime Date { get; set; }

    public virtual Topic Topic { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
