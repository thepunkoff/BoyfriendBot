using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Hosted.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace BoyfriendBot.Domain.Services.Hosted
{
    public class ListeningService : IHostedService, IListeningService
    {
        private readonly TelegramBotClient _botClient;
        private readonly ILogger<ListeningService> _logger;
        private readonly IBoyfriendBotDbContext _dbContext;
        private readonly IUserStorage _userStorage;
        private readonly ListeningServiceAppSettings _appSettings;
        private readonly IMonitoringManager _monitoringManager;

        public ListeningService(
              ITelegramClientWrapper telegramClientWrapper
            , ILogger<ListeningService> logger
            , IBoyfriendBotDbContext dbContext
            , IUserStorage userCache
            , IOptions<ListeningServiceAppSettings> appSettings
            , IMonitoringManager monitoringManager
            )
        {
            _botClient = telegramClientWrapper.Client;
            _logger = logger;
            _dbContext = dbContext;
            _userStorage = userCache;
            _appSettings = appSettings.Value;
            _monitoringManager = monitoringManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_appSettings.ListeningServiceOn)
            {
                return;
            }

            _logger.LogInformation("Started");

            _botClient.OnMessage += OnMessage;

            _botClient.StartReceiving();

            _monitoringManager.Listening = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _botClient.StopReceiving();

            _monitoringManager.Listening = false;

            _logger.LogInformation("Stopped");
        }

        async void OnMessage(object sender, MessageEventArgs eventArgs)
        {
            var userName = eventArgs.Message.From.Username;
            if (!_userStorage.HasUser(userName))
            {
                var chatId = eventArgs.Message.Chat.Id;
                
                await _userStorage.AddNewUser(userName, chatId);
            }
            await _botClient.SendTextMessageAsync(eventArgs.Message.Chat, "Прости, я тебя пока что не понимаю!");
        }
    }
}
