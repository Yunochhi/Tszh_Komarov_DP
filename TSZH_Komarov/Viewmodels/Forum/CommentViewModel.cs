namespace TSZH_Komarov.Viewmodels.Forum
{
    public class CommentViewModel
    {
        public int TopicCommentId { get; set; }
        public int TopicId { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int VotesCount { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
    }
}
