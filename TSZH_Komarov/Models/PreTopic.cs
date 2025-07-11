﻿using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class PreTopic
{
    public int PreTopicId { get; set; }

    public int ForumCategorieId { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Description { get; set; } = null!;

    public int TszhId { get; set; }
}
