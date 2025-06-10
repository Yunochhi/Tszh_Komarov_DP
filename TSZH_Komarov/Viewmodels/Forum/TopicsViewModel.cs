using TSZH_Komarov.Models;

namespace TSZH_Komarov.Viewmodels.Forum
{
    public class TopicsViewModel
    {
        public ForumCategory Category { get; set; }
        public List<TopicViewModelItem> Topics { get; set; }
    }
}
