﻿using AutoMapper;
using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Hosted.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BoyfriendBot.Domain.Services.Hosted
{
    public class ListeningService : IHostedService, IListeningService
    {
        private readonly TelegramBotClient _botClient;
        private readonly ILogger<ListeningService> _logger;
        private readonly IBoyfriendBotDbContextFactory _dbContextFactory;
        private readonly ICommandProcessor _commandProcessor;
        private readonly IUserStorage _userStorage;
        private readonly ListeningServiceAppSettings _appSettings;
        private readonly IMonitoringManager _monitoringManager;
        private readonly IMapper _mapper;
        private readonly IEventManager _eventManager;

        public ListeningService(
              ITelegramBotClientWrapper telegramClientWrapper
            , ILogger<ListeningService> logger
            , IBoyfriendBotDbContextFactory dbContextFactory
            , ICommandProcessor commandProcessor
            , IUserStorage userCache
            , IOptions<ListeningServiceAppSettings> appSettings
            , IMonitoringManager monitoringManager
            , IMapper mapper
            , IEventManager eventManager
            )
        {
            _botClient = telegramClientWrapper.Client;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _commandProcessor = commandProcessor;
            _userStorage = userCache;
            _appSettings = appSettings.Value;
            _monitoringManager = monitoringManager;
            _mapper = mapper;
            _eventManager = eventManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_appSettings.ListeningServiceOn)
            {
                return;
            }

            _botClient.OnMessage += OnMessage;
            _botClient.OnCallbackQuery += OnCallbackQuery;

            _botClient.StartReceiving(new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery });

            _monitoringManager.Listening = true;

            _logger.LogInformation("Started");
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            var data = e.CallbackQuery.Data;

            if (data.StartsWith("/"))
            {
                await _commandProcessor.ProcessCommand(data.TrimStart('/'), e.CallbackQuery.Message.Chat.Id);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _botClient.StopReceiving();

            _monitoringManager.Listening = false;

            _logger.LogInformation("Stopped");
        }

        async void OnMessage(object sender, MessageEventArgs eventArgs)
        {
            var message = eventArgs.Message;
            var userId = message.From.Id;

            LogMessage(message);

            if (!_userStorage.HasUser(userId))
            {
                var mappedUser = _mapper.Map<UserDbo>(message);

                await _userStorage.AddNewUser(mappedUser);

                _eventManager.InvokeNewUser(mappedUser);
            }

            if (message.Text.StartsWith("/"))
            {
                await _commandProcessor.ProcessCommand(message.Text.TrimStart('/'), message.Chat.Id);
            }
        }

        private void LogMessage(Message message)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Incoming message ({message.MessageId}) from {message.From.FirstName} {message.From.LastName} ({message.From.Id})");

            if (message.ReplyToMessage != null)
            {
                var mId = message.ReplyToMessage.MessageId;

                var fn = message.ReplyToMessage.From.FirstName;
                var ln = message.ReplyToMessage.From.LastName;
                var id = message.ReplyToMessage.From.Id;

                sb.AppendLine($"Replied to message ({mId}) from {fn} {ln} ({id}): \"{message.ReplyToMessage.Text}\"");
            }

            if (message.ForwardFrom != null)
            {
                var fn = message.ForwardFrom.FirstName;
                var ln = message.ForwardFrom.LastName;
                var id = message.ForwardFrom.Id;

                sb.AppendLine($"Forwarded from {fn} {ln} ({id}): \"{message.Text}\"");
            }

            if (message.Text != null)
            {
                sb.AppendLine($"Text: \"{message.Text}\"");
            }
            if (message.Sticker != null)
            {
                sb.AppendLine($"Sticker: \"{message.Sticker.Emoji}\"");
            }
            if (message.Photo != null)
            {
                sb.AppendLine($"Picture recieved. Picture caption: \"{message.Caption}\"");
            }
            if (message.Audio != null)
            {
                sb.AppendLine($"Audio recieved. Duration: \"{message.Audio.Duration}\"");
            }
            if (message.Voice != null)
            {
                sb.AppendLine($"Voice recieved. Duration: \"{message.Voice.Duration}\"");
            }
            if (message.VideoNote != null)
            {
                sb.AppendLine($"Video note recieved. Duration: \"{message.VideoNote.Duration}\"");
            }
            if (message.Venue != null)
            {
                sb.AppendLine($"Audio recieved. Address: \"{message.Venue.Address}\"");
            }
            if (message.Document != null)
            {
                sb.AppendLine($"Document recieved. File name: \"{message.Document.FileName}\"");
            }
            
            var log = sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());

            _logger.LogInformation(log);
        }
    }
}
