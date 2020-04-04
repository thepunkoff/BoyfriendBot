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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private CancellationTokenSource _cts;

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

            _logger.LogInformation($"Initializing scheduled messaging service...");

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
                SendWakeUpMessage(cancellationToken);
            }

            try
            {
                _eventManager.NewUserEvent += OnNewUser;
                _eventManager.RescheduleClickedEvent += OnRescheduleClicked;

                await RescheduleMessages(cancellationToken);

                _logger.LogInformation($"Started");

                _cts = new CancellationTokenSource();
                var task = Task.Run(() => Run(_cts.Token));

                return;
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"StartAsync: {ex.ToString()}");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                _cts.Cancel();

                _eventManager.NewUserEvent -= OnNewUser;
                _eventManager.RescheduleClickedEvent -= OnRescheduleClicked;

                _monitoringManager.SchedulingMessages = false;

                _logger.LogInformation($"Stopped");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"StopAsync: {ex.ToString()}");
                throw;
            }
        }

        private async void SendWakeUpMessage(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var rng = new Random();
            if (!(rng.NextDouble() < 0.05d))
            {
                await _bulkMessagingTelegramClient.SendWakeUpMessageToAllUsersAsync(cancellationToken);
            }
            else
            {
                await _bulkMessagingTelegramClient.SendWakeUpMessageToAllUsersAsync(cancellationToken);
            }
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                _monitoringManager.SchedulingMessages = true;

                var dayBorder = DateTime.Now.Date.AddDays(1);
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (DateTime.Now > dayBorder)
                    {
                        await RescheduleMessages(cancellationToken);

                        dayBorder = DateTime.Now.Date.AddDays(1);
                    }

                    await SendMessagesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Run: {ex.ToString()}");
                throw;
            }
        }

        private async Task SendMessagesAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var messageDateTimes = await _messageSchedule.GetAllScheduledMessageTimes(cancellationToken);

            foreach (var messageTime in messageDateTimes)
            {
                var now = DateTime.Now;

                if (now >= messageTime)
                {
                    var message = await _messageSchedule.GetScheduledMessage(messageTime, cancellationToken);

                    await _telegramClient.SendMessageAsync(
                        category: Enum.Parse<MessageCategory>(messageTime.PartOfDay().Name.ToUpperInvariant()),
                        message.Type,
                        message.Rarity,
                        message.ChatId);
                    
                    await _messageSchedule.RemoveScheduledMessage(messageTime, cancellationToken);
                }
            }
        }

        private async Task ScheduleSpecialMessages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var partsOfDay = new List<PartOfDay>();

            var now = DateTime.Now;
            var partOfDay = now.PartOfDay();

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

                        var schedulingTime = _dateTimeGenerator.GenerateRandomDateTimeWithinRange(new DateTimeRange(start, end));
                        var message = new ScheduledMessage
                        {
                            ChatId = user.ChatId,
                            Rarity = _rarityRoller.RollRarityForUser(user),
                            Type = Enum.Parse<MessageType>(messageType.Type.ToUpperInvariant()),
                            Time = schedulingTime,
                            Category = Enum.Parse<MessageCategory>(schedulingTime.PartOfDay().Name.ToUpperInvariant())
                        };

                        await _messageSchedule.AddScheduledMessage(message, cancellationToken);

                        scheduledCount++;
                    }
                }
            }

            _logger.LogInformation($"{scheduledCount} special messages were scheduled.");
        }

        private async Task ScheduleStandardMessages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var now = DateTime.Now;

            var partsOfDay = new List<PartOfDay>();

            var partOfDay = now.PartOfDay();

            partsOfDay.Add(partOfDay);
            partsOfDay.AddRange(partOfDay.Rest);

            var users = await _userStorage.GetAllUsersForScheduledMessagesNoTracking();

            var scheduledCount = 0;
            var totalOverriddenCount = 0;
            var overrideRng = new Random();
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
                        Time = dateTime,
                        Category = Enum.Parse<MessageCategory>(dateTime.PartOfDay().Name.ToUpperInvariant())
                    }).ToList();

                    var overridenCount = TryOverrideByAny(messages, overrideRng);

                    totalOverriddenCount += overridenCount;

                    await _messageSchedule.AddScheduledMessageRange(messages, cancellationToken);

                    scheduledCount += messages.Count();
                }
            }

            _logger.LogInformation($"{scheduledCount} standard messages were scheduled. {totalOverriddenCount} of them overridden to \"ANY\" category.");
        }

        private int TryOverrideByAny(List<ScheduledMessage> messages, Random overrideRng)
        {
            var overridenCount = 0;
            foreach (var message in messages)
            {
                var overrideCategoryByAny = overrideRng.NextDouble() <= _appSettings.OverrideCategoryChanceNormalized;

                if (overrideCategoryByAny)
                {
                    message.Category = MessageCategory.ANY;
                    overridenCount++;
                }
            }

            return overridenCount;
        }

        private async Task RescheduleMessages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogInformation($"Scheduling new messages...");

            await _messageSchedule.RemoveAllScheduledMessages(cancellationToken);

            await ScheduleStandardMessages(cancellationToken);

            await ScheduleSpecialMessages(cancellationToken);
#if DEBUG
            _logger.LogInformation($"{_messageSchedule.ToString()}");
#endif
        }

        #region EventHandlers
        private async void OnNewUser(object sender, UserDbo e)
        {
            await StopAsync(default);

            await StartAsync(default);
        }

        private async void OnRescheduleClicked(object sender, EventArgs e)
        {
            await StopAsync(default);

            await StartAsync(default);
        }
        #endregion
    }
}
