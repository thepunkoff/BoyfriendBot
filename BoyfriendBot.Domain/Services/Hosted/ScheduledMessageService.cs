using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Core.Extensions;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Hosted.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services.Hosted
{
    public class ScheduledMessageService : IHostedService, IScheduledMessageService
    {
        private readonly ScheduledMessageServiceAppSettings _appSettings;
        private readonly IBulkMessagingTelegramClient _bulkMessagingTelegramClient;
        private readonly ITelegramClient _telegramClient;
        private readonly ILogger<ScheduledMessageService> _logger;
        private readonly IMonitoringManager _monitoringManager;
        private readonly IDateTimeGenerator _dateTimeGenerator;
        private readonly IUserStorage _userStorage;
        private readonly IMessageSchedule _messageSchedule;
        private readonly IRarityRoller _rarityRoller;
        private readonly IEventManager _eventManager;

        private Dictionary<PartOfDay, int> MessageCounts { get; set; }

        public ScheduledMessageService(
              IOptions<ScheduledMessageServiceAppSettings> appSettings
            , IBulkMessagingTelegramClient bulkMessagingTelegramClient
            , ITelegramClient telegramClient
            , ILogger<ScheduledMessageService> logger
            , IMonitoringManager monitoringManager
            , IDateTimeGenerator dateTimeGenerator
            , IUserStorage userStorage
            , IMessageSchedule messageSchedule
            , IRarityRoller rarityRoller
            , IEventManager eventManager
            )
        {
            _appSettings = appSettings.Value;
            _bulkMessagingTelegramClient = bulkMessagingTelegramClient;
            _telegramClient = telegramClient;
            _logger = logger;
            _monitoringManager = monitoringManager;
            _dateTimeGenerator = dateTimeGenerator;
            _userStorage = userStorage;
            _messageSchedule = messageSchedule;
            _rarityRoller = rarityRoller;
            _eventManager = eventManager;

            _logger.LogInformation("Initializing scheduled messaging service...");

            // get message count from personal settings
            MessageCounts = new Dictionary<PartOfDay, int>
            {
                [PartOfDay.Night] = _appSettings.NightMessagesCount,
                [PartOfDay.Morning] = _appSettings.MorningMessagesCount,
                [PartOfDay.Afternoon] = _appSettings.AfternoonMessagesCount,
                [PartOfDay.Evening] = _appSettings.EveningMessagesCount
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_appSettings.ScheduledMessageServiceOn)
            {
                return;
            }

            if (_appSettings.WakeUpMessageOn)
            {
                SendWakeUpMessage();
            }

            _eventManager.NewUserEvent += OnNewUser;
            _eventManager.RescheduleClickedEvent += OnRescheduleClicked;

            await RescheduleMessages();

            _logger.LogInformation("Started");

            var task = Task.Run(Run);

            return;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _monitoringManager.SchedulingMessages = false;

            _logger.LogInformation("Stopped");
        }
        private async void SendWakeUpMessage()
        {
            var rng = new Random();
            if (!(rng.NextDouble() < 0.05d))
            {
                await _bulkMessagingTelegramClient.SendWakeUpMessageToAllUsersAsync();
            }
            else
            {
                await _bulkMessagingTelegramClient.SendWakeUpMessageToAllUsersAsync();
            }
        }

        public async Task Run()
        {
            try
            {
                _monitoringManager.SchedulingMessages = true;

                var dayBorder = DateTime.Now.Date.AddDays(1);
                while (true)
                {
                    if (DateTime.Now > dayBorder)
                    {
                        await RescheduleMessages();

                        dayBorder = DateTime.Now.Date.AddDays(1);
                    }

                    await SendMessagesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        private async Task SendMessagesAsync()
        {
            foreach (var message in await _messageSchedule.GetAllScheduledMessages())
            {
                var now = DateTime.Now;

                if (now >= message.Time)
                {
                    await _telegramClient.SendMessageAsync(message.Time.PartOfDay().Name, message.Type, message.Rarity, message.ChatId);

                    _logger.LogInformation($"Scheduled message sent." +
                        $"ChatId: {message.ChatId}," +
                        $"Category: {message.Time.PartOfDay().Name}," +
                        $"Type: {message.Type}," +
                        $"Rarity: {message.Rarity}.");

                    await _messageSchedule.RemoveScheduledMessage(message);
                }
            }
        }

        private async Task ScheduleSpecialMessages()
        {
            var partsOfDay = new List<PartOfDay>();

            var now = DateTime.Now;
            var partOfDay = now.PartOfDay();
            var currentStart = now;
            var currentEnd = now.Date + partOfDay.End;

            partsOfDay.Add(partOfDay);
            partsOfDay.AddRange(partOfDay.Rest);

            var users = await _userStorage.GetAllUsersForScheduledMessagesNoTracking();
            
            var scheduledCount = 0;
            foreach (var user in users)
            {
                foreach (var part in partsOfDay)
                {
                    var messageTypesForPart = Const.PartOfDay.SpecialMessageTypes[part];
                    foreach (var messageType in messageTypesForPart)
                    {
                        var start = now.Date.AddSeconds(messageType.Range.Start.TotalSeconds);

                        if (now > start) start = now;

                        var end = now.Date.AddSeconds(messageType.Range.End.TotalSeconds);

                        if (now > end) continue;

                        var message = new ScheduledMessage
                        {
                            ChatId = user.ChatId,
                            Rarity = _rarityRoller.RollRarityForUser(user),
                            Type = Enum.Parse<MessageType>(messageType.Type.ToUpperInvariant()),
                            Time = _dateTimeGenerator.GenerateRandomDateTimeWithinRange(new DateTimeRange(start, end)),
                        };

                        await _messageSchedule.AddScheduledMessage(message);

                        scheduledCount++;
                    }
                }
            }

            _logger.LogInformation($"{scheduledCount} special messages were scheduled.");
        }

        private async Task ScheduleStandardMessages()
        {
            var now = DateTime.Now;

            var partsOfDay = new List<PartOfDay>();

            var partOfDay = now.PartOfDay();

            partsOfDay.Add(partOfDay);
            partsOfDay.AddRange(partOfDay.Rest);

            var users = await _userStorage.GetAllUsersForScheduledMessagesNoTracking();

            var scheduledCount = 0;
            foreach (var user in users)
            {
                foreach (var part in partsOfDay)
                {
                    var start = now.Date.AddSeconds(part.Start.TotalSeconds);

                    if (now > start) start = now;

                    var end = now.Date.AddSeconds(part.End.TotalSeconds);

                    var range = new DateTimeRange(start, end);

                    List<DateTime> dateTimes = null;
                    if (range.Difference <= TimeSpan.FromHours(_appSettings.ThresholdInHours))
                    {
                        dateTimes = _dateTimeGenerator.GenerateDateTimesWithinRange(range, messageCount: 1);
                    }
                    else
                    {
                        dateTimes = _dateTimeGenerator.GenerateDateTimesWithinRange(range, MessageCounts[part]);
                    }

                    var messages = dateTimes.Select(dateTime => new ScheduledMessage
                    {
                        Type = MessageType.STANDARD,
                        Rarity = _rarityRoller.RollRarityForUser(user),
                        ChatId = user.ChatId,
                        Time = dateTime
                    });

                    await _messageSchedule.AddScheduledMessageRange(messages);

                    scheduledCount += messages.Count();
                }
            }

            _logger.LogInformation($"{scheduledCount} standard messages were scheduled.");
        }

        private async Task RollRarities(IEnumerable<ScheduledMessage> messages)
        {
            var userDbos = await _userStorage.GetUserByChatIdRangeNoTracking(messages.Select(x => x.ChatId));

            foreach (var message in messages)
            {
                var user = userDbos.Where(x => x.ChatId == message.ChatId).FirstOrDefault();
                message.Rarity = _rarityRoller.RollRarityForUser(user);
            }
        }

        #region EventHandlers
        private async void OnNewUser(object sender, UserDbo e)
        {
            await RescheduleMessages();
        }

        private async void OnRescheduleClicked(object sender, EventArgs e)
        {
            await RescheduleMessages();
        }

        private async Task RescheduleMessages()
        {
            _logger.LogInformation("Scheduling new messages...");

            await _messageSchedule.RemoveAllScheduledMessages();

            await ScheduleStandardMessages();

            await ScheduleSpecialMessages();

            var messages = await _messageSchedule.GetAllScheduledMessages();

            messages.Sort((x, y) =>
                x.Time > y.Time
                    ? 1
                    : x.Time < y.Time
                        ? -1
                        : 0
                    );

            _logger.LogInformation(_messageSchedule.ToString());
        }
        #endregion
    }
}
