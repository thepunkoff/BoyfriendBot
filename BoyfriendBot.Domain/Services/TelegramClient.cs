using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Infrastructure.ResultProcessing;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
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

        public async Task<SendMessageResult> SendMessageAsync(MessageCategory category, MessageType type, MessageRarity rarity, long chatId)
        {
            var redalertMessage = false;

            var message = await _messageTextProvider.GetMessage(category, type, rarity, chatId);

            if (message == null)
            {
                message.Text = Const.RedAlertMessage;
                redalertMessage = true;
            }

            try
            {
                if (message.Image == null)
                {
                    await _telegramBotClientWrapper.Client.SendTextMessageAsync(chatId, message.Text);
                }
                else
                {
                    await _telegramBotClientWrapper.Client.SendPhotoAsync(chatId, message.Image, caption: message.Text);
                }
            }
            catch (ApiRequestException ex)
            {
                if (ex.Message.Contains("blocked"))
                {
                    var result = SendMessageResult.CreateBlocked();
                    return result;
                }

                _logger.LogError(ex.ToString());

                return SendMessageResult.CreateUnknown();
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

            return SendMessageResult.CreateSuccess();
        }
    }
}
