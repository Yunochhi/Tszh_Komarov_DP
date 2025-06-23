using System.Text;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Viewmodels.Service;

namespace TSZH_Komarov.Services
{
    public class ServicesService
    {
        private TszhKomarovContext context;
        private UserService userService;
        private TelegramService telegramService;
        public ServicesService(TszhKomarovContext context, UserService userService, TelegramService telegramService    )
        {
            this.context = context;
            this.userService = userService;
            this.telegramService = telegramService;
        }
        public List<ServiceType> LoadServices()
        {
            try{
                var services = context.ServiceTypes.ToList();
                return services;
            }
            catch{
                return null;
            }
        }

        public List<ServiceRequest> LoadRequests(string filter)
        {
            var userId = userService.GetCurrUser().UserId;
            var currAppart = userService.GetCurrAppartment().ApartmentId;

            try
            {
                var requests = context.ServiceRequests
               .Include(r => r.ServiceType)
               .Where(r => r.UserId == userId && r.ApartmentId == currAppart &&
                   (filter == "all" ||
                   (filter == "active" && r.Status == "В обработке") ||
                   (filter == "completed" && r.Status == "Выполнено") ||
                   (filter == "cancelled" && r.Status == "Отменено"))) 
               .OrderByDescending(r => r.CreatedDate)
               .ToList();


                return requests;
            }
            catch{
                return null;
            }
        }

        public ServiceViewModel GetServiceView(string filter)
        {
            var model = new ServiceViewModel
            {
                CurrentFilter = filter,
                Services = LoadServices(),
                Requests = LoadRequests(filter),
                ActiveRequests = LoadRequests("active")
            };
            return model;
        }

        public string AddRequest(int serviceTypeId, string? note)
        {
            var user = userService.GetCurrUser();
            var currAppart = userService.GetCurrAppartment();
            var currTszh = userService.GetCurrTszh().TszhId;

            var anyRequest = context.ServiceRequests.Any(c => c.UserId == user.UserId 
                        && c.ServiceTypeId == serviceTypeId 
                        && c.Status == "В обработке" 
                        && c.ApartmentId == currAppart.ApartmentId);
            
            if (anyRequest) {
                return "Выбранная служба уже в заявке!";
            }
            var request = new ServiceRequest
            {
                ServiceTypeId = serviceTypeId,
                UserId = user.UserId,
                Status = "В обработке",
                CreatedDate = DateTime.Now,
                ApartmentId = currAppart.ApartmentId,
                Note = note
            };

            var requestName = context.ServiceTypes.Find(serviceTypeId);
            var admin = context.AppUsers.Include(a => a.Apartments).ThenInclude(h => h.House).FirstOrDefault(t => t.Apartments.Any(h => h.House.TszhId == currTszh) && t.Role == 1);

            try
            {
                context.ServiceRequests.Add(request);

                if (admin != null)
                {
                    var notification = new Notification()
                    {
                        UserId = admin.UserId,
                        Topic = $"Новая заявка пользователя!",
                        Description =
                        $"{user.Fullname} оставил заявку на вызов службы \"{requestName.Name}\", " +
                        $"для квартиры по адресу: {currAppart.House.Address}, " +
                        $"кв. {currAppart.Number}. " +
                        $"Статус заявки: \"В обработке\". ",
                        CreatedDate = DateTime.UtcNow,
                        TszhId = currTszh
                    };
                    context.Notifications.Add(notification);
                }
                string msg = 
                    $"{user.Fullname}, вы оставили заявку на вызов службы \"{requestName.Name}\", " +
                    $"для квартиры по адресу: {currAppart.House.Address}, " +
                    $"кв. {currAppart.Number}. " +
                    (string.IsNullOrEmpty(note) ? "" : $"Примечание к заявке: \"{note}\". ") +
                    $"Статус заявки: \"В обработке\". " +
                    $"Если у вас остались вопросы по поводу оставленной заявки, обратитесь к администратору вашей ТСЖ.";

                var userNotification = new Notification()
                {
                    UserId = user.UserId,
                    Topic = $"Новая заявка!",
                    Description = msg,
                    CreatedDate = DateTime.UtcNow,
                    TszhId = currTszh
                };
                context.Notifications.Add(userNotification);
                if (!user.ChatId.IsNullOrEmpty())
                    telegramService.SendMessageAsync(user.ChatId, msg);

                context.SaveChanges();
                return "Заявка успешно добавлена!";
            }
            catch{ 
                return "Произошла ошибка! Попробуйте еще раз!";
            }
              
        }

        public List<ServiceRequestsGroupViewModel> LoadActiveRequests()
        {
            var requests = context.ServiceRequests
           .Include(r => r.ServiceType)
           .Include(r => r.User)
           .Where(r => r.Status != "Выполнено" && r.Status != "Отменено")
           .ToList();

            var groupedRequests = requests
            .GroupBy(r => r.ServiceType)
            .Select(g => new ServiceRequestsGroupViewModel
            {
                ServiceTypeId = g.Key.ServiceTypeId,
                ServiceTypeName = g.Key.Name,
                Requests = g.Select(r => new ServiceRequestViewModel
                {
                    ServiceRequestId = r.ServiceRequestId,
                    UserFullName = r.User.Fullname,
                    CreatedDate = r.CreatedDate,
                    CurrentStatus = r.Status,
                    Address = $"{context.Apartments.Include(h => h.House).FirstOrDefault(a => a.ApartmentId == r.ApartmentId).House.Address}, кв {context.Apartments.Include(h => h.House).FirstOrDefault(a => a.ApartmentId == r.ApartmentId).Number}",
                    Note = r.Note,
                    AdminComment = r.AdminComment
                }).ToList()
            }).ToList();

            return groupedRequests;
        }
        public string UpdateRequestStatus(int requestId, string newStatus, string? adminComment)
        {
            var currAppart = userService.GetCurrAppartment().ApartmentId;
            var currTszh = userService.GetCurrTszh().TszhId;

            string msg = "";
            var request = context.ServiceRequests.Include(u => u.User)
                .Include(s => s.ServiceType)
                .Where(r => r.ServiceRequestId == requestId)
                .FirstOrDefault();

            var currApart = context.Apartments.Include(h => h.House).Where(c => c.ApartmentId == request.ApartmentId)
                .FirstOrDefault();
            string topic = $"Статус вашей заявки изменён!";
            string desc = $"{request.User.Fullname}, уведомляем вас, что статус заявки \"{request.ServiceType.Name}\" " +
                                    $"для квартиры по адресу: {currApart.House.Address}, Кв. {currApart.Number} " +
                                    $"изменён на \"{newStatus}\"! " +
                                    $"Комментарий администратора: {adminComment}. " +
                                    $"Если у вас остались вопросы по поводу изменения статуса, обратитесь к администратору вашей ТСЖ.";

            var requestApart = context.Apartments.Include(h => h.House)
                .FirstOrDefault(a => a.ApartmentId == request.ApartmentId);
            try
            {
                request.Status = newStatus;
                request.AdminComment = adminComment;
                msg = "Статус заявки успешно обновлен";
                context.SaveChanges();

                var notification = new Notification()
                {
                    UserId = request.UserId,
                    Topic = topic,
                    Description = desc,
                    CreatedDate = DateTime.UtcNow,
                    TszhId = requestApart.House.TszhId
                };
                
                if (!request.User.ChatId.IsNullOrEmpty())
                    telegramService.SendMessageAsync(request.User.ChatId, desc);

                context.Notifications.Add(notification);
                context.SaveChanges();
            }
            catch
            {
                msg = "Произошла неизвестная ошибка! попробуйте ещё раз!";
            } 
            return msg;
        }
    }
}
