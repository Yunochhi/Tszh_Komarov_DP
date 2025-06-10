using System.Text;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Viewmodels;

namespace TSZH_Komarov.Services
{
    public class ServicesService
    {
        Data.TszhKomarovContext context;
        private UserService userService;
        public ServicesService(Data.TszhKomarovContext context, UserService userService)
        {
            this.context = context;
            this.userService = userService;
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
            var currTszh = userService.GetCurrTszh().TszhId;

            try
            {
                var requests = context.ServiceRequests
               .Include(r => r.ServiceType)
               .Where(r => r.UserId == userId && r.TszhId == currTszh &&
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

        public string AddRequest(int serviceTypeId)
        {
            var user = userService.GetCurrUser();
            var currTszh = userService.GetCurrTszh().TszhId;

            var anyRequest = context.ServiceRequests.Any(c => c.UserId == user.UserId && c.ServiceTypeId == serviceTypeId && c.Status == "В обработке" && c.TszhId == currTszh);
            
            if (anyRequest) {
                return "Выбранная служба уже в заявке!";
            }
            var request = new ServiceRequest
            {
                ServiceTypeId = serviceTypeId,
                UserId = user.UserId,
                Status = "В обработке",
                CreatedDate = DateTime.Now,
                TszhId = currTszh,
            };

            var requestName = context.ServiceTypes.Find(serviceTypeId);
            var admin = context.AppUsers.Include(a => a.Apartments).ThenInclude(h => h.House).FirstOrDefault(t => t.Apartments.Any(h => h.House.TszhId == currTszh));

           
            try
            {
                if (admin != null)
                {
                    var notification = new Notification()
                    {
                        UserId = admin.UserId,
                        Topic = $"Новая заявка пользователя!",
                        Description = $"{user.Fullname} оставил заявку на вызов службы \"{requestName.Name}\"\n. Статус заявки: \"В обработке\"",
                        CreatedDate = DateTime.UtcNow,
                        TszhId = currTszh
                    };
                    context.Notifications.Add(notification);
                }

                context.ServiceRequests.Add(request);
                context.SaveChanges();

                return "Заявка успешно добавлена!";
            }
            catch{ 
                return "Произошла ошибка! Попробуйте еще раз!1";
            }
              
        }
    }
}
