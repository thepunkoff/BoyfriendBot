using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Services
{
    public class TelegramClient : ITelegramClient
    {
        private readonly ITelegramBotClientWrapper _telegramBotClientWrapper;
        private readonly IMessageTextProvider _messageTextProvider;

        public TelegramClient(
              ITelegramBotClientWrapper telegramBotClientWrapper
            , IMessageTextProvider messageTextProvider
            )
        {
            _telegramBotClientWrapper = telegramBotClientWrapper;
            _messageTextProvider = messageTextProvider;
        }

        public async Task SendMessageAsync(PartOfDay partOfDay, MessageType type, long chatId)
        {
            var message = _messageTextProvider.GetMessage(partOfDay.Name, type);

            await _telegramBotClientWrapper.Client.SendTextMessageAsync(chatId, message);
        }
    }
}
