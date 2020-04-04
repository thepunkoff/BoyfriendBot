using AutoMapper;
using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Hosted.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
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
        private readonly IBotMessageProvider _botMessageProvider;
        private readonly IRarityRoller _rarityRoller;

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
            , IBotMessageProvider botMessageProvider
            , IRarityRoller rarityRoller
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
            _botMessageProvider = botMessageProvider;
            _rarityRoller = rarityRoller;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_appSettings.ListeningServiceOn)
            {
                return;
            }

            try
            {
                _botClient.OnMessage += OnMessage;
                _botClient.OnCallbackQuery += OnCallbackQuery;


                _botClient.StartReceiving(new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery });

                _monitoringManager.Listening = true;

                _logger.LogInformation($"[{Const.Serilog.ListeningService}] Started");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"StartAsync: {ex.ToString()}");
                throw;
            }
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                var data = e.CallbackQuery.Data;
                var messageId = e.CallbackQuery.Message.MessageId.ToString();

                if (data.StartsWith("/"))
                {
                    var message = data.TrimStart('/') + $" {messageId}";

                    await _commandProcessor.ProcessCommand(message.Trim(), e.CallbackQuery.Message.Chat.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"OnCallbackQuery: {ex.ToString()}");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _botClient.StopReceiving();

                _monitoringManager.Listening = false;

                _logger.LogInformation($"[{Const.Serilog.ListeningService}] Stopped");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"StopAsync: {ex.ToString()}");
                throw;
            }
        }

        async void OnMessage(object sender, MessageEventArgs eventArgs)
        {
            try
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
                
                var realUser = await _userStorage.GetUserByChatIdNoTracking(message.Chat.Id);

                if (message.Text == null)
                    _logger.LogError("FWAfawf");

                if (message.Text.StartsWith("/"))
                {
                    await _commandProcessor.ProcessCommand(message.Text.TrimStart('/'), message.Chat.Id);
                }
                else
                {
                    var responseMessage = await _botMessageProvider.GetMessage(
                        MessageCategory.SIMPLERESPONSE,
                        Models.MessageType.STANDARD,
                        rarity: _rarityRoller.RollRarityForUser(realUser),
                        message.Chat.Id);

                    await _botClient.SendTextMessageAsync(message.Chat.Id, responseMessage.Text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"OnMessage: {ex.ToString()}");
            }
        }

        private void LogMessage(Message message)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"[{Const.Serilog.ListeningService}] Incoming message ({message.MessageId}) from {message.From.FirstName} {message.From.LastName} ({message.From.Id})");

            if (message.ReplyToMessage != null)
            {
                var mId = message.ReplyToMessage.MessageId;

                var fn = message.ReplyToMessage.From.FirstName;
                var ln = message.ReplyToMessage.From.LastName;
                var id = message.ReplyToMessage.From.Id;

                sb.AppendLine($"[{Const.Serilog.ListeningService}] Replied to message ({mId}) from {fn} {ln} ({id}): \"{message.ReplyToMessage.Text}\"");
            }

            if (message.ForwardFrom != null)
            {
                var fn = message.ForwardFrom.FirstName;
                var ln = message.ForwardFrom.LastName;
                var id = message.ForwardFrom.Id;

                sb.AppendLine($" [{Const.Serilog.ListeningService}] Forwarded from {fn} {ln} ({id}): \"{message.Text}\"");
            }

            if (message.Text != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Text: \"{message.Text}\"");
            }
            if (message.Sticker != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Sticker: \"{message.Sticker.Emoji}\"");
            }
            if (message.Photo != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Picture recieved. Picture caption: \"{message.Caption}\"");
            }
            if (message.Audio != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Audio recieved. Duration: \"{message.Audio.Duration}\"");
            }
            if (message.Voice != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Voice recieved. Duration: \"{message.Voice.Duration}\"");
            }
            if (message.VideoNote != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Video note recieved. Duration: \"{message.VideoNote.Duration}\"");
            }
            if (message.Venue != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Audio recieved. Address: \"{message.Venue.Address}\"");
            }
            if (message.Document != null)
            {
                sb.AppendLine($"[{Const.Serilog.ListeningService}] Document recieved. File name: \"{message.Document.FileName}\"");
            }
            
            var log = sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());

            _logger.LogInformation(log);
        }
    }
}
