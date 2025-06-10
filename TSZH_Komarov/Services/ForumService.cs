using System.ComponentModel.Design;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Viewmodels.Forum;

namespace TSZH_Komarov.Services
{
    public class ForumService
    {
        private Data.TszhKomarovContext context;
        private UserService userService;

        public ForumService(Data.TszhKomarovContext context, UserService userService)
        {
            this.context = context;
            this.userService = userService;
        }
        public List<ForumCategory> GetForumCategories()
        {
            var forumCategories = context.ForumCategories.ToList();
            return forumCategories;
        }

        public TopicsViewModel GetTopicsFromCategory(int categoryId)
        {
            var category = context.ForumCategories.Find(categoryId);
            var currTszh = userService.GetCurrTszh().TszhId;

            var topics = context.Topics
                .Where(t => t.ForumCategorieId == categoryId && t.TszhId == currTszh)
                .Select(t => new TopicViewModelItem
                {
                    ForumCategorieId = t.ForumCategorieId,
                    TopicId = t.TopicId,
                    Name = t.Name,
                    Date = t.Date,
                    AuthorId = t.User.UserId,
                    CommentCount = t.TopicComments.Count()
                })
                .OrderByDescending(t => t.Date)
                .ToList();

            var model = new TopicsViewModel
            {
                Category = category,
                Topics = topics
            };

            return model;
        }

        public TopicViewModel GetTopic(int topicId)
        {
            var topic = context.Topics
             .Include(t => t.User).Include(t => t.ForumCategorie)
             .FirstOrDefault(t => t.TopicId == topicId);
  
            var comments = context.TopicComments.Include(c => c.Topic)
                .Where(c => c.TopicId == topicId)
                .OrderBy(c => c.Date)
                .Select(c => new CommentViewModel
                {
                    TopicId = c.TopicId,
                    TopicCommentId = c.TopicCommentId,
                    Description = c.Description,
                    Date = c.Date,
                    VotesCount = c.VotesCount,
                    AuthorId = c.UserId,
                    AuthorName = c.User.Fullname
                })
                .ToList();

            var model = new TopicViewModel
            {
                Topic = new TopicViewModelItem
                {
                    ForumCategorieId = topic.ForumCategorieId,
                    TopicId = topic.TopicId,
                    Name = topic.Name,
                    Date = topic.Date,
                    AuthorId = topic.UserId,
                    AuthorName = topic.User.Fullname,
                    Description = topic.Description
                },     
                Comments = comments
            };

            return model;
        }
        public bool AddComment(int topicId, string description)
        {
            var userId = userService.GetCurrUser().UserId;

            var comment = new TopicComment
            {
                TopicId = topicId,
                UserId = userId,
                Description = description,
                Date = DateTime.Now,
                VotesCount = 0
            };
            try
            {
                context.TopicComments.Add(comment);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
          
        }
        public TopicComment AddVote(int commentId)
        {
            var comment = context.TopicComments.Find(commentId);
 
            try
            {
                comment.VotesCount++;
                context.SaveChanges();
                return comment;
            }
            catch
            {
                return null;
            }
        }
        public PreTopic AddTopic(int categoryId, string name, string description)
        {
            var userId = userService.GetCurrUser().UserId;
            var currTszh = userService.GetCurrTszh().TszhId;

            var topic = new PreTopic
            {
                ForumCategorieId = categoryId,
                UserId = userId,
                Name = name,
                Description = description,
                Date = DateTime.Now,
                TszhId = currTszh,
            };
            try
            {
                context.PreTopics.Add(topic);
                context.SaveChanges();
                return topic;
            }
            catch
            {
                return null;
            }
        }
        public List<PreTopicViewModel> getPreTopics()
        {
            var userId = userService.GetCurrUser().UserId;
            var currTszh = userService.GetCurrTszh().TszhId;

            var preTopics = context.PreTopics
                .Select(p => new PreTopicViewModel
                {
                    PreTopicId = p.PreTopicId,
                    ForumCategorieId = p.ForumCategorieId,
                    UserId = p.UserId,
                    Name = p.Name,
                    Description = p.Description,
                    TszhId = currTszh
                })
                .ToList();

            return preTopics;
        }
        public string ModeratePreTopic(string actionType, PreTopicViewModel model)
        {
            var preTopic = context.PreTopics.FirstOrDefault(c => c.PreTopicId == model.PreTopicId);
            string msg;
            try
            {
                if (actionType == "publish")
                {
                    var newTopic = new Topic
                    {
                        Name = model.Name,
                        Description = model.Description,
                        ForumCategorieId = model.ForumCategorieId,
                        UserId = model.UserId,
                        Date = DateTime.UtcNow,
                        TszhId = model.TszhId,
                    };
                    try
                    {
                        context.Topics.Add(newTopic);
                        context.PreTopics.Remove(preTopic);
                        context.SaveChanges();
                        msg = "Тема добавлена!";
                    }
                    catch
                    {
                        msg = "Произошла ошибка при добавлении!";
                    }
                    return msg;
                }
                else
                {
                    context.PreTopics.Remove(preTopic);
                    context.SaveChanges();
                    return "Предложенная тема отклонена!";
                }
            }
            catch
            {
                return "Произошла ошибка! попробуйте еще раз!";
            }
        }
        public string DeleteTopic(int topicId)
        {
            string msg;

            var topic = context.Topics.Find(topicId);
            var comments = context.TopicComments.Where(c => c.TopicId == topicId);

            try
            {
                context.TopicComments.RemoveRange(comments);
                context.Topics.Remove(topic);
                context.SaveChanges();
                return "Тема успешно удалена!";
            }
            catch
            {
                return "Произошла ошибка при удалении темы!";
            }
        }
        public TopicComment DeleteComment(int commentId)
        {
            string msg;

            var comment = context.TopicComments.Find(commentId);
      
            try
            {
                context.TopicComments.Remove(comment);
                context.SaveChanges();
                return comment;
            }
            catch
            {
                return null;
            }
        }
    }
}
