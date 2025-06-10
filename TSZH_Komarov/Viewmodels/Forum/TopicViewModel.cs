using TSZH_Komarov.Models;

namespace TSZH_Komarov.Viewmodels.Forum
{
    public class TopicViewModel
    {
        public TopicViewModelItem Topic { get; set; }
        public List<CommentViewModel> Comments { get; set; }
    }
  
    public class TopicViewModelItem()
    {
        public int TopicId { get; set; }
        public int ForumCategorieId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int CommentCount { get; set; }
        public string Description { get; set; }

    }
}
