namespace TSZH_Komarov.Viewmodels.Forum
{
    public class PreTopicViewModel
    {
        public int PreTopicId { get; set; }

        public int TszhId { get; set; }
        public int ForumCategorieId { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;
    }
}
