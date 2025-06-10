using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class ForumCategory
{
    public int ForumCategorieId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
