using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Models;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels;

namespace TSZH_Komarov.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private AnnouncementService announcementService;

        public HomeController(ILogger<HomeController> logger, AnnouncementService announcementService)
        {
            _logger = logger;
            this.announcementService = announcementService;
        }

        public IActionResult Index(string sortOrder)
        { 
            if (User.Identity.IsAuthenticated)
            {
                var query = announcementService.GetAnnouncements(sortOrder);

                ViewData["CurrentSort"] = sortOrder ?? "date";
                return View(query);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }

        }
        public IActionResult PreAnnouncement()
        {
            var model = announcementService.GetPreAnnouncements();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PreAnnouncement(string actionType, PreAnnouncementViewModel model)
        {
            string msg = announcementService.AddAnnouncement(actionType, model);
          
            TempData["Message"] = msg;

            return RedirectToAction("PreAnnouncement", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> AddAnnouncement(string topic, string description, int priority)
        {
            var announc = announcementService.AddPreAnnouncement(topic, description, priority);
            if (topic != null)
            {
                TempData["Message"] = "Ваше объявление отправлено на модерацию!";
                return RedirectToAction("Index");
            }
            TempData["Message"] = "При отправке произошла ошибка!";
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
