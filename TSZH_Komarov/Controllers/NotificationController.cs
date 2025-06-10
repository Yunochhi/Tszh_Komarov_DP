using Microsoft.AspNetCore.Mvc;
using TSZH_Komarov.Services;

namespace TSZH_Komarov.Controllers
{
    public class NotificationController : Controller
    {
        private NotificationService notificationService;

        public NotificationController(NotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        public IActionResult Notifications()
        {
            var notifs = notificationService.GetNotifications();
            return View(notifs);
        }
    }
}
