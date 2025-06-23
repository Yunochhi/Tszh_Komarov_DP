namespace TSZH_Komarov.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Telegram.Bot;
    using Telegram.Bot.Exceptions;
    using Telegram.Bot.Types;
    using TSZH_Komarov.Data;

    public class TelegramPollingService : BackgroundService
    {
        private readonly ILogger<TelegramPollingService> logger;
        private readonly IServiceProvider serviceProvider;
        private TelegramBotClient botClient;
        private int _lastUpdateId = 0;

        public TelegramPollingService(ILogger<TelegramPollingService> logger,IServiceProvider serviceProvider,IConfiguration configuration)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;

            var botToken = configuration["Telegram:BotToken"];
            if (string.IsNullOrEmpty(botToken))
            {
                throw new ArgumentNullException("Telegram:BotToken не настроен в конфигурации");
            }

            this.botClient = new TelegramBotClient(botToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Служба опроса Telegram запущена");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var updates = await botClient.GetUpdates(
                        offset: _lastUpdateId + 1,
                        timeout: 30, // Секунды
                        cancellationToken: stoppingToken);

                    foreach (var update in updates)
                    {
                        _lastUpdateId = update.Id;
                        await ProcessUpdate(update, stoppingToken);
                    }
                }
                catch (ApiRequestException ex)
                {
                    logger.LogError(ex, "Ошибка API Telegram: {Error}", ex.Message);
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка в службе опроса Telegram");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            logger.LogInformation("Служба опроса Telegram остановлена");
        }

        private async Task ProcessUpdate(Update update, CancellationToken ct)
        {
            logger.LogInformation($"Получено обновление: {update.Id}");

            // Создаем новый scope для каждого обновления
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TszhKomarovContext>();
            var telegramService = scope.ServiceProvider.GetRequiredService<TelegramService>();

            if (update.Message == null || update.Message.Chat == null) return;

            var chatId = update.Message.Chat.Id.ToString();
            var messageText = update.Message.Text;

            // Обработка команды /start
            if (!string.IsNullOrEmpty(messageText) && messageText.StartsWith("/start"))
            {
                logger.LogInformation($"Обработка команды /start: {messageText}");

                var token = messageText.Split(' ').LastOrDefault();

                if (!string.IsNullOrEmpty(token))
                {
                    var (success, userId) = telegramService.ProcessStartCommand(token);

                    if (success && userId.HasValue)
                    {
                        var user = await context.AppUsers.FindAsync(userId.Value);
                        if (user != null)
                        {
                            // Проверка на дубликат
                            var existingUser = await context.AppUsers
                                .FirstOrDefaultAsync(u => u.ChatId == chatId);

                            if (existingUser != null && existingUser.UserId != user.UserId)
                            {
                                logger.LogWarning($"ChatId {chatId} уже привязан к другому пользователю");
                                await telegramService.SendMessageAsync(chatId, "❌ Этот Telegram-аккаунт уже привязан к другому пользователю.", ct);
                                return;
                            }

                            user.ChatId = chatId;
                            await context.SaveChangesAsync();

                            logger.LogInformation($"ChatId {chatId} сохранен для пользователя {user.UserId}");
                            await telegramService.SendMessageAsync(chatId, "✅ Ваш аккаунт успешно привязан! " +
                                "Теперь вы будете получать уведомления здесь.", ct);
                            return;
                        }
                    }
                }
                await telegramService.SendMessageAsync(chatId, "❌ Не удалось привязать аккаунт. Пожалуйста, " +
                    "получите новую ссылку в приложении.", ct);
            }
            else {
                await telegramService.SendMessageAsync(chatId, "ℹ️ Привет! Я бот для уведомлений. " +
                    "Для привязки аккаунта используйте ссылку из приложения.", ct);
            }
        } 
    }
}
