using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;

namespace TSZH_Komarov.Services
{
    public class TelegramService
    {
        private readonly IMemoryCache cache;
        private readonly ILogger<TelegramService> logger;
        private readonly IConfiguration config;
        private TelegramBotClient botClient;

        public TelegramService(IMemoryCache cache,ILogger<TelegramService> logger,IConfiguration config)
        {
            this.cache = cache;
            this.logger = logger;
            this.config = config;

            // Инициализация бота только при первом использовании
            var botToken = config["Telegram:BotToken"];
            if (!string.IsNullOrEmpty(botToken)){
                botClient = new TelegramBotClient(botToken);
            }
            else{
                logger.LogWarning("Telegram BotToken не настроен. Отправка сообщений невозможна.");
            }
        }

        public string GenerateBotLink(int userId)
        {
            var token = Guid.NewGuid().ToString("N");
            cache.Set(token, userId, TimeSpan.FromMinutes(10));

            var botUsername = config["Telegram:BotUsername"];
            if (string.IsNullOrEmpty(botUsername))
            {
                throw new InvalidOperationException("BotUsername не настроен");
            }

            return $"https://t.me/{botUsername}?start={token}";
        }

        public (bool success, int? userId) ProcessStartCommand(string token)
        {
            if (cache.TryGetValue(token, out int userId))
            {
                cache.Remove(token);
                return (true, userId);
            }
            return (false, null);
        }

        public async Task SendMessageAsync(string chatId, string message, CancellationToken ct = default)
        {
            if (botClient == null)
            {
                logger.LogWarning("Попытка отправки сообщения без инициализированного бота");
                return;
            }

            if (string.IsNullOrEmpty(chatId))
            {
                logger.LogWarning("Не указан ChatId для отправки сообщения");
                return;
            }

            try
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: message,
                    cancellationToken: ct);

                logger.LogInformation($"Сообщение отправлено в Telegram чат {chatId}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Ошибка отправки сообщения в чат {chatId}");
            }
        }
    }
}
