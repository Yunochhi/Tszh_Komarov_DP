using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Models;

namespace TSZH_Komarov.Services
{
    public class NotificationService
    {
        private Data.TszhKomarovContext context;
        private UserService userService;

        public NotificationService(Data.TszhKomarovContext context, UserService userService)
        {
            this.context = context;
            this.userService = userService;
        }

        public List<Notification?> GetNotifications()
        {
            int userId = userService.GetCurrUser().UserId;
            int tshzId = userService.GetCurrTszh().TszhId;

            var notifications = context.Notifications.Where(u => u.UserId == userId && u.TszhId == tshzId).ToList();

            return notifications;
        }
    }
}
