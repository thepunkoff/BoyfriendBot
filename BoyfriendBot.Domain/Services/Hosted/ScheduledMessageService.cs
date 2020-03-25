using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Core.Extensions;
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

            await _messageSchedule.RemoveAllScheduledMessages();

            await ScheduleStandardMessages();

            await ScheduleSpecialMessages();

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
                await _bulkMessagingTelegramClient.SendWakeUpMessageToAllUsersAsync(MessageType.PLAIN);
            }
            else
            {
                await _bulkMessagingTelegramClient.SendWakeUpMessageToAllUsersAsync(MessageType.SPECIAL);
            }
        }

        public async Task Run()
        {
            _monitoringManager.SchedulingMessages = true;

            var dayBorder = DateTime.Now.Date.AddDays(1);
            while (true)
            {
                if (DateTime.Now > dayBorder)
                {
                    await _messageSchedule.RemoveAllScheduledMessages();

                    await ScheduleStandardMessages();

                    await ScheduleSpecialMessages();

                    dayBorder = DateTime.Now.Date.AddDays(1);
                }

                await SendMessagesAsync();
            }
        }

        private async Task SendMessagesAsync()
        {
            foreach (var message in await _messageSchedule.GetAllScheduledMessages())
            {
                var now = DateTime.Now;

                if (now >= message.Time)
                {
                    await _telegramClient.SendMessageAsync(now.PartOfDay().Name, message.Type, message.ChatId);

                    _logger.LogInformation($"Scheduled message sent. ChatId: {message.ChatId}, Category: {now.PartOfDay().Name}, Type: {message.Type}.");

                    await _messageSchedule.RemoveScheduledMessage(message);
                }
            }
        }

        private async Task ScheduleSpecialMessages()
        {
            var users = await _userStorage.GetAllUsersForScheduledMessages();

            foreach (var chatId in users.Select(x => x.ChatId))
            {
                var rng = new Random();
                if (!(rng.NextDouble() < 0.05d))
                {
                    continue;
                }

                var allScheduledMessages = await _messageSchedule.GetAllScheduledMessages();
                allScheduledMessages.Sort((x, y) =>
                x.Time > y.Time
                    ? 1
                    : x.Time < y.Time
                        ? -1
                        : 0
                    );

                var dateTime = _dateTimeGenerator.GenerateRandomDateTimeWithinRange(
                new DateTimeRange(allScheduledMessages.First().Time, allScheduledMessages.Last().Time));

                var message = new ScheduledMessage(MessageType.SPECIAL, chatId, dateTime);
                await _messageSchedule.AddScheduledMessage(message);
            }
        }

        private async Task ScheduleStandardMessages()
        {
            var partOfDay = DateTime.Now.PartOfDay();

            var users = await _userStorage.GetAllUsersForScheduledMessages();

            var scheduledCount = 0;
            foreach (var chatId in users.Select(x => x.ChatId))
            {
                scheduledCount =+ await ScheduleForCurrentPart(partOfDay, chatId);

                scheduledCount += await ScheduleForRestParts(partOfDay, chatId);
            }

            _logger.LogInformation($"{scheduledCount} messages were scheduled.");
        }

        private async Task<int> ScheduleForCurrentPart(PartOfDay partOfDay, long chatId)
        {
            var scheduledCount = 0;

            var now = DateTime.Now;
            var currentStart = now;
            var currentEnd = now.Date + partOfDay.End;
            var restTimeRange = new DateTimeRange(currentStart, currentEnd);

            if (restTimeRange.Difference <= TimeSpan.FromHours(_appSettings.ThresholdInHours))
            {
                var dateTimes = _dateTimeGenerator.GenerateDateTimesWithinRange(restTimeRange, messageCount: 1);

                var messages = dateTimes.Select(dateTime => new ScheduledMessage(MessageType.PLAIN, chatId, dateTime));

                await _messageSchedule.AddScheduledMessageRange(messages);

                scheduledCount += messages.Count();
            }
            else
            {
                var dateTimes = _dateTimeGenerator.GenerateDateTimesWithinRange(restTimeRange, MessageCounts[partOfDay]);

                var messages = dateTimes.Select(dateTime => new ScheduledMessage(MessageType.PLAIN, chatId, dateTime));

                await _messageSchedule.AddScheduledMessageRange(messages);

                scheduledCount += messages.Count();
            }

            return scheduledCount;
        }

        private async Task<int> ScheduleForRestParts(PartOfDay partOfDay, long chatId)
        {
            var scheduledCount = 0;

            foreach (var p in partOfDay.Rest)
            {
                var restStart = p.Date.AddSeconds(p.Start.TotalSeconds);
                var restEnd = p.Date.AddSeconds(p.End.TotalSeconds);

                var range = new DateTimeRange(restStart, restEnd);

                var dateTimes = _dateTimeGenerator.GenerateDateTimesWithinRange(range, MessageCounts[p]);

                var messages = dateTimes.Select(dateTime => new ScheduledMessage(MessageType.PLAIN, chatId, dateTime));

                await _messageSchedule.AddScheduledMessageRange(messages);

                scheduledCount += messages.Count();
            }

            return scheduledCount;
        }
    }
}
