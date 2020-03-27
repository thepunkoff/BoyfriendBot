using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Commands
{
    public class SendMenuCommand : Command
    {
        private readonly TelegramBotClient _botClient;
        private readonly IInlineKeyboardMenuParser _menuParser;

        public SendMenuCommand(
              ITelegramBotClientWrapper botClient
            , IInlineKeyboardMenuParser menuParser
            )
        {
            _botClient = botClient.Client;
            _menuParser = menuParser;
        }

        public override async Task Execute(long chatId, params string[] args)
        {
            var menuId = args[0];

            var menu = _menuParser.Parse(menuId);

            await _botClient.SendTextMessageAsync(
                chatId,
                menu.Text,
                replyMarkup: menu.ReplyMarkup
            );
        }
    }
}
