using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SendMenuCommand> _logger;

        public SendMenuCommand(
              ITelegramBotClientWrapper botClient
            , IInlineKeyboardMenuParser menuParser
            , ILogger<SendMenuCommand> logger
            )
        {
            _botClient = botClient.Client;
            _menuParser = menuParser;
            _logger = logger;
        }

        public override async Task Execute(long chatId, params string[] args)
        {
            var menuId = args[0];
            var menu = _menuParser.Parse(menuId);


            var messageId = 0;
            if (args.Length == 2)
            {
                int.TryParse(args[1], out messageId);
            }

            if (messageId == 0)
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    menu.Text,
                    replyMarkup: menu.ReplyMarkup
                );
            }
            else
            {
                await _botClient.EditMessageTextAsync(
                    chatId,
                    messageId: messageId,
                    menu.Text,
                    replyMarkup: menu.ReplyMarkup
                );
            }
        }
    }
}
