using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class TelegramClient : ITelegramClient
    {
        private readonly ITelegramBotClientWrapper _telegramBotClientWrapper;
        private readonly IMessageTextProvider _messageTextProvider;
        private readonly ILogger<TelegramClient> _logger;


        public TelegramClient(
              ITelegramBotClientWrapper telegramBotClientWrapper
            , IMessageTextProvider messageTextProvider
            , ILogger<TelegramClient> logger
            )
        {
            _telegramBotClientWrapper = telegramBotClientWrapper;
            _messageTextProvider = messageTextProvider;
            _logger = logger;
        }

        public async Task SendMessageAsync(MessageCategory category, MessageType type, MessageRarity rarity, long chatId)
        {
            var redalertMessage = false;

            var message = await _messageTextProvider.GetMessage(category, type, rarity);

            if (message == null)
            {
                message = Const.RedAlertMessage;
                redalertMessage = true;
            }

            await _telegramBotClientWrapper.Client.SendTextMessageAsync(chatId, message);

            if (redalertMessage)
            {
                _logger.LogInformation($"RedAlert message sent. ChatId: {chatId}.");
            }
            else
            {
                _logger.LogInformation($"Message sent. " +
                            $"ChatId: {chatId}, " +
                            $"Category: {category}, " +
                            $"Type: {type}, " +
                            $"Rarity: {rarity}.");
            }
        }
    }
}
