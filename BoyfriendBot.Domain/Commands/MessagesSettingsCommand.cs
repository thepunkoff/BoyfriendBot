using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace BoyfriendBot.Domain.Commands
{
    public class MessagesSettingsCommand : Command
    {
        private readonly IUserStorage _userStorage;
        private readonly ILogger<MessagesSettingsCommand> _logger;
        private readonly ITelegramBotClientWrapper _botClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBoyfriendBotDbContextFactory _dbContextFactory;

        public override long ChatId { get; protected set; }

        public MessagesSettingsCommand(
              IUserStorage userStorage
            , ILogger<MessagesSettingsCommand> logger
            , ITelegramBotClientWrapper botClient
            , IServiceProvider serviceProvider
            , IBoyfriendBotDbContextFactory dbContextFactory
            )
        {
            _userStorage = userStorage;
            _logger = logger;
            _botClient = botClient;
            _serviceProvider = serviceProvider;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task Execute(long chatId)
        {
            ChatId = chatId;

            _botClient.Client.OnCallbackQuery += OnCallbackQueryEventHandler;

            await _botClient.Client.SendTextMessageAsync(
                chatId: ChatId,
                text: "Выберите настройку!",
                replyMarkup: new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Уведомления", "settings mes rem"),
                        InlineKeyboardButton.WithCallbackData("Ежедневные сообщения", "settings mes sched")
                    }));

        }

        private async void OnCallbackQueryEventHandler(object sender, CallbackQueryEventArgs e)
        {   
            var query = e.CallbackQuery;
            if (query.Data == "settings mes rem")
            {
                await _botClient.Client.SendTextMessageAsync(
                chatId: ChatId,
                text: "Выберите значение!",
                replyMarkup: new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Выключить", "settings mes rem off"),
                        InlineKeyboardButton.WithCallbackData("Включить", "settings mes rem on")
                    }));
            }
            if (query.Data == "settings mes sched")
            {
                await _botClient.Client.SendTextMessageAsync(
                chatId: ChatId,
                text: "Выберите значение!",
                replyMarkup: new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Выключить", "settings mes sched off"),
                        InlineKeyboardButton.WithCallbackData("Включить", "settings mes sched on")
                    }));
            }
            if (query.Data == "settings mes sched off")
            {
                using (var context = _dbContextFactory.Create())
                {
                    var settings = await context.UserSettings.Where(x => x.UserId == ChatId).FirstOrDefaultAsync();
                    settings.RecieveScheduled = false;
                    await context.SaveChangesAsync();
                }
                _logger.LogInformation($"Chat: {ChatId}. RecieveScheduled = false");
                await _botClient.Client.SendTextMessageAsync(ChatId, "Вы успешно выключили ежедневные сообщения!");
                _botClient.Client.OnCallbackQuery -= OnCallbackQueryEventHandler;
            }
            if (query.Data == "settings mes sched on")
            {
                using (var context = _dbContextFactory.Create())
                {
                    var settings = await context.UserSettings.Where(x => x.UserId == ChatId).FirstOrDefaultAsync();
                    settings.RecieveScheduled = true;
                    await context.SaveChangesAsync();
                }
                _logger.LogInformation($"Chat: {ChatId}. RecieveScheduled = true");
                await _botClient.Client.SendTextMessageAsync(ChatId, "Вы успешно включили ежедневные сообщения!");
                _botClient.Client.OnCallbackQuery -= OnCallbackQueryEventHandler;
            }
            if (query.Data == "settings mes rem off")
            {
                using (var context = _dbContextFactory.Create())
                {
                    var settings = await context.UserSettings.Where(x => x.UserId == ChatId).FirstOrDefaultAsync();
                    settings.RecieveReminders = false;
                    await context.SaveChangesAsync();
                }
                _logger.LogInformation($"Chat: {ChatId}. RecieveReminders= false");
                await _botClient.Client.SendTextMessageAsync(ChatId, "Вы успешно выключили уведомления!");
                _botClient.Client.OnCallbackQuery -= OnCallbackQueryEventHandler;
            }
            if (query.Data == "settings mes rem on")
            {
                using (var context = _dbContextFactory.Create())
                {
                    var settings = await context.UserSettings.Where(x => x.UserId == ChatId).FirstOrDefaultAsync();
                    settings.RecieveReminders = true;
                    await context.SaveChangesAsync();
                }
                _logger.LogInformation($"Chat: {ChatId}. RecieveReminders = true");
                await _botClient.Client.SendTextMessageAsync(ChatId, "Вы успешно включили уведомления!");
                _botClient.Client.OnCallbackQuery -= OnCallbackQueryEventHandler;
            }
        }
    }
}
