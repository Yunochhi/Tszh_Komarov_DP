using System;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using TSZH_Komarov.Data;
using TSZH_Komarov.Models;

namespace TSZH_Komarov.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider services;
        private readonly ILogger<ReminderBackgroundService> logger;

        public ReminderBackgroundService(IServiceProvider services, ILogger<ReminderBackgroundService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Служба напоминаний запущена");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TszhKomarovContext>();

                    /*var today = new DateTime(2024, 6, 17);*/
                    var today = DateTime.Today;
                    var targetDay = 20;

                    var users = await dbContext.AppUsers
                        .Include(u => u.Apartments)
                            .ThenInclude(a => a.House)
                                .ThenInclude(h => h.Tszh)
                        .Where(u => u.ReminderDaysBefore >= 0)
                        .ToListAsync(stoppingToken);

                    foreach (var user in users)
                    {
                        try
                        {
                            int reminderDay = targetDay - user.ReminderDaysBefore;
                            bool isReminderDay = today.Day == reminderDay;
                            bool notSentThisMonth = user.LastReminderSent == null ||
                                                  user.LastReminderSent.Value.Month != today.Month ||
                                                  user.LastReminderSent.Value.Year != today.Year;

                            if (isReminderDay && notSentThisMonth)
                            {
                                await SendReminder(dbContext, user, stoppingToken);
                                user.LastReminderSent = today;

                                await dbContext.SaveChangesAsync(stoppingToken);
                                logger.LogInformation($"Напоминание сохранено для {user.Email}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Ошибка для пользователя {user.UserId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Общая ошибка в службе напоминаний");
                }

                /* await Task.Delay(TimeSpan.FromHours(1), stoppingToken);*/

                var nextRun = DateTime.Today.AddDays(1).AddHours(9);
                var delay = nextRun - DateTime.Now;
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task SendReminder(TszhKomarovContext dbContext, AppUser user, CancellationToken stoppingToken)
        {
            var tszhGroups = user.Apartments
                        .Where(a => a.House?.Tszh != null)
                        .GroupBy(a => a.House.TszhId)
                        .ToList();

            string message = user.ReminderDaysBefore == 0
                ? $"{user.Fullname}, напоминаем, что сегодня последний день подачи показаний счетчиков!"
                : $"{user.Fullname}, напоминаем, что вам будет необходимо передать показания счетчиков через " +
                $"{user.ReminderDaysBefore} дн. (20 числа)";

            foreach (var group in tszhGroups)
            {
                var tszhId = group.Key;

                var notification = new Notification
                {
                    UserId = user.UserId,
                    Topic = "Напоминание о подаче показаний!",
                    Description = message,
                    CreatedDate = DateTime.Now,
                    TszhId = tszhId
                };

                await dbContext.Notifications.AddAsync(notification);
            }

            if (!string.IsNullOrEmpty(user.ChatId))
            {
                try
                {
                    using var scope = services.CreateScope();
                    var telegramService = scope.ServiceProvider.GetRequiredService<TelegramService>();
                    await telegramService.SendMessageAsync(user.ChatId, message, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Ошибка при отправке Telegram сообщения пользователю {user.UserId}");
                }
            }
        }
    }
}

