using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using BoyfriendBot.Domain.Services.Hosted.Interfaces;
using Telegram.Bot.Types.Enums;

namespace BoyfriendBot.Domain.Commands
{
    public class ChoseSettingsCommand : Command
    {
        private readonly IUserStorage _userStorage;
        private readonly ILogger<ChoseSettingsCommand> _logger;
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;

        public ChoseSettingsCommand(
              IUserStorage userStorage
            , ILogger<ChoseSettingsCommand> logger
            , ITelegramBotClientWrapper botClient
            , IServiceProvider serviceProvider
            )
        {
            _userStorage = userStorage;
            _logger = logger;
            _botClient = botClient.Client;
            _serviceProvider = serviceProvider;
        }

        public long UserId { get; set; }
        public override long ChatId { get; protected set; }
        public override async Task Execute(long chatId)
        {
            ChatId = chatId;

            _botClient.OnCallbackQuery += OnCallbackQueryEventHandler;

            _botClient.StartReceiving(allowedUpdates: new UpdateType[] { UpdateType.CallbackQuery });

            var me = await _botClient.GetMeAsync();
            _logger.LogInformation($"{me.Id}");

            await _botClient.SendTextMessageAsync(
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
