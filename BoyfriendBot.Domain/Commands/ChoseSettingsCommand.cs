using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.DependencyInjection;

namespace BoyfriendBot.Domain.Commands
{
    public class ChoseSettingsCommand : Command
    {
        private readonly IUserStorage _userStorage;
        private readonly ILogger<ChoseSettingsCommand> _logger;
        private readonly ITelegramClientWrapper _botClient;
        private readonly IServiceProvider _serviceProvider;

        public ChoseSettingsCommand(
              IUserStorage userStorage
            , ILogger<ChoseSettingsCommand> logger
            , ITelegramClientWrapper botClient
            , IServiceProvider serviceProvider
            )
        {
            _userStorage = userStorage;
            _logger = logger;
            _botClient = botClient;
            _serviceProvider = serviceProvider;
        }

        public long UserId { get; set; }
        public override long ChatId { get; protected set; }
        public override async Task Execute(long chatId)
        {
            ChatId = chatId;

            _botClient.Client.OnCallbackQuery += OnCallbackQueryEventHandler;

            await _botClient.Client.SendTextMessageAsync(
                chatId: ChatId,
                text: "Выбери категорию",
                replyMarkup: new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Получение сообщений", "settings mes"),
                    }));

        }

        private async void OnCallbackQueryEventHandler(object sender, CallbackQueryEventArgs e)
        {
            var query = e.CallbackQuery;

            if (query.Data == "settings mes")
            {
                var command = _serviceProvider.GetService<MessagesSettingsCommand>();

                await command.Execute(ChatId);
            }
        }
    }
}
