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

        public async Task SendMessageAsync(string category, MessageType type, MessageRarity rarity, long chatId)
        {
            var message = _messageTextProvider.GetMessage(category, type, rarity);

            await _telegramBotClientWrapper.Client.SendTextMessageAsync(chatId, message);
        }
    }
}
