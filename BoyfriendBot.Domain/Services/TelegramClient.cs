using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services
{
    public class TelegramClient : ITelegramClient
    {
        private readonly ITelegramBotClientWrapper _telegramBotClientWrapper;
        private readonly IBotMessageProvider _messageTextProvider;
        private readonly ILogger<TelegramClient> _logger;


        public TelegramClient(
              ITelegramBotClientWrapper telegramBotClientWrapper
            , IBotMessageProvider messageTextProvider
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

            var message = await _messageTextProvider.GetMessage(category, type, rarity, chatId);

            if (message == null)
            {
                message.Text = Const.RedAlertMessage;
                redalertMessage = true;
            }

            if (message.ImageUrl == null)
            {
                await _telegramBotClientWrapper.Client.SendTextMessageAsync(chatId, message.Text);
            }
            else
            {
                await _telegramBotClientWrapper.Client.SendPhotoAsync(chatId, message.ImageUrl, caption: message.Text);
            }

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
