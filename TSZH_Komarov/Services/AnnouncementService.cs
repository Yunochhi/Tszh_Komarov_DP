using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;
using TSZH_Komarov.Services;
using TSZH_Komarov.Viewmodels;

namespace TSZH_Komarov.Services
{
    public class AnnouncementService
    {
        private Data.TszhKomarovContext context;
        private UserService userService;

        public AnnouncementService(Data.TszhKomarovContext context, UserService userService)
        {
            this.context = context;
            this.userService = userService;
        }

        public List<Announcement?> GetAnnouncements(string sortOrder)
        {
            int tshzId = userService.GetCurrTszh().TszhId;

            var query = context.Announcements.Include(t => t.Tszh).Where(c => c.TszhId == tshzId).AsQueryable();

            query = sortOrder switch
            {
                "priority" => query.OrderBy(a => a.Priority).ThenByDescending(a => a.Date),
                _ => query.OrderByDescending(a => a.Date)
            };

            return query.ToList();
        }
        public List<PreAnnouncementViewModel> GetPreAnnouncements()
        {
            var preAnnouncements = context.PreAnnouncements
            .Select(a => new PreAnnouncementViewModel
            {
                PreAnnouncementId = a.PreAnnouncementId,
                TszhId = a.TszhId,
                Topic = a.Topic,
                Description = a.Description,
                Priority = a.Priority
            })
            .ToList();

            return preAnnouncements;
        }
        public PreAnnouncement AddPreAnnouncement(string topic, string description, int priority)
        {
            var user = userService.GetCurrUser();
            var tszhId = user.Apartments.FirstOrDefault().House.TszhId;

            var preAnnouncement = new PreAnnouncement
            {
                TszhId = tszhId,
                Topic = topic,
                Description = description,
                Priority = priority,
                Date = DateTime.Now
            };

            try
            {
                context.PreAnnouncements.Add(preAnnouncement);
                context.SaveChanges();
                return preAnnouncement;
            }
            catch
            {
                return null;
            }
        }
        
        
        public string AddAnnouncement(string actionType, PreAnnouncementViewModel model)
        {
            var preAnnouncement = context.PreAnnouncements.FirstOrDefault(c => c.PreAnnouncementId == model.PreAnnouncementId);
            string msg;
            try
            {
                if (actionType == "publish")
                {
                    var newAnnouncement = new Announcement
                    {
                        TszhId = model.TszhId,
                        Topic = model.Topic,
                        Description = model.Description,
                        Priority = model.Priority,
                        Date = DateTime.UtcNow
                    };
                    try
                    {
                        context.Announcements.Add(newAnnouncement);
                        context.PreAnnouncements.Remove(preAnnouncement);
                        context.SaveChanges();
                        msg = "Объявление добавлено!";
                    }
                    catch
                    {
                        msg = "Произошла ошибка при добавления!";
                    }
                    return msg;
                }
                else
                {
                    context.PreAnnouncements.Remove(preAnnouncement);
                    context.SaveChanges();
                    return "Предложенное объявление отклонено!!";
                }      
            }
            catch
            {
                return "Произошла ошибка! попробуйте еще раз!";
            }
        }
    }
}
