using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Models;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels.Forum;

namespace TSZH_Komarov.Controllers
{
    public class ForumController : Controller
    {
        private ForumService forumService;
        public ForumController(ForumService forumService)
        {
            this.forumService = forumService;
        }

        public IActionResult ForumCategories()
        {
            var forumCategories = forumService.GetForumCategories();
            return View(forumCategories);
        }

        public IActionResult ForumTopics(int categoryId)
        {
            var topicsFromCategory = forumService.GetTopicsFromCategory(categoryId);
            return View(topicsFromCategory);
        }

        public IActionResult ForumTopic(int topicId)
        {
            var topic = forumService.GetTopic(topicId);
            return View(topic);
        }

        public IActionResult ForumPreTopic()
        {
            var model = forumService.getPreTopics();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int topicId, string description)
        {
            if (forumService.AddComment(topicId, description))
            {
                return RedirectToAction("ForumTopic", new { topicId });
            }
            return RedirectToAction("ForumTopic", new { topicId });
        }

        [HttpPost]
        public async Task<IActionResult> Vote(int commentId)
        {
            var comment = forumService.AddVote(commentId);
            if (comment != null)
            {
                return RedirectToAction("ForumTopic", new { topicId = comment.TopicId });
            }
            return RedirectToAction("ForumTopic", new { topicId = comment.TopicId });
        }

        [HttpPost]
        public async Task<IActionResult> addTopic(int categoryId, string name, string description)
        {
            var topic = forumService.AddTopic(categoryId, name, description);
            if (topic != null)
            {
                TempData["Message"] = "Ваша тема отправлена на модерацию!";
                return RedirectToAction("ForumTopics", new { categoryId });
            }
            TempData["Message"] = "При отправке произошла ошибка!";
            return RedirectToAction("ForumTopics", new { categoryId });
        }

        [HttpPost]
        public IActionResult ModeratePreTopic(string actionType, PreTopicViewModel model)
        {

            string msg = forumService.ModeratePreTopic(actionType, model);

            TempData["Message"] = msg;

            return RedirectToAction("ForumPreTopic");
        }

        [HttpPost]
        public IActionResult DeleteTopic(int topicId, int categoryId)
        {
            string request = forumService.DeleteTopic(topicId);
            TempData["Message"] = request;
            return RedirectToAction("ForumTopics", new { categoryId });
        }

        [HttpPost]
        public IActionResult DeleteComment(int commentId)
        {

            var comment = forumService.DeleteComment(commentId);

            if (comment != null){
                TempData["Message"] = "Комментарий успешно удален!";
            }
            else TempData["Message"] = "Произошла ошибка при удалении комментария!";

            return RedirectToAction("ForumTopic", new { comment.TopicId });
        }
    }
}
