using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class Topic
{
    public int TopicId { get; set; }

    public int ForumCategorieId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Description { get; set; } = null!;

    public int TszhId { get; set; }

    public virtual ForumCategory ForumCategorie { get; set; } = null!;

    public virtual ICollection<TopicComment> TopicComments { get; set; } = new List<TopicComment>();

    public virtual AppUser User { get; set; } = null!;
}
