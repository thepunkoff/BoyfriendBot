using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Core.Extensions;
using BoyfriendBot.Domain.Services.Hosted.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services.Hosted
{
    public class ScheduledMessageService : IHostedService, IScheduledMessageService
    {
        private readonly ScheduledMessageServiceAppSettings _appSettings;
        private readonly IBulkMessagingTelegramClient _bulkMessagingTelegramClient;
        private readonly ILogger<ScheduledMessageService> _logger;
        private readonly IMonitoringManager _monitoringManager;
        private readonly IDateTimeGenerator _dateTimeGenerator;

        private Dictionary<PartOfDay, int> MessageCounts { get; set; }

        private List<DateTime> MessageSchedule { get; set; }

        public ScheduledMessageService(
              IOptions<ScheduledMessageServiceAppSettings> appSettings
            , IBulkMessagingTelegramClient bulkMessagingTelegramClient
            , ILogger<ScheduledMessageService> logger
            , IMonitoringManager monitoringManager
            , IDateTimeGenerator dateTimeGenerator
            )
        {
            _appSettings = appSettings.Value;
            _bulkMessagingTelegramClient = bulkMessagingTelegramClient;
            _logger = logger;
            _monitoringManager = monitoringManager;
            _dateTimeGenerator = dateTimeGenerator;

            _logger.LogInformation("Initializing scheduled messaging service...");

            // get message count from personal settings
            MessageCounts = new Dictionary<PartOfDay, int>
            {
                [PartOfDay.Night] = _appSettings.NightMessagesCount,
                [PartOfDay.Morning] = _appSettings.MorningMessagesCount,
                [PartOfDay.Afternoon] = _appSettings.AfternoonMessagesCount,
                [PartOfDay.Evening] = _appSettings.EveningMessagesCount
            };

            MessageSchedule = new List<DateTime>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_appSettings.ScheduledMessageServiceOn)
            {
                return Task.CompletedTask;
            }

            if (_appSettings.WakeUpMessageOn)
            {
                SendWakeUpMessage();
            }

            GenerateNewMessageDateTimes();

            _logger.LogInformation("Started");

            var task = Task.Run(Run);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _monitoringManager.SchedulingMessages = false;

            _logger.LogInformation("Stopped");
        }
        private async void SendWakeUpMessage()
        {
            await _bulkMessagingTelegramClient.SendWakeUpMessageToAllUsersAsync();
        }

        public async Task Run()
        {
            _monitoringManager.SchedulingMessages = true;

            var dayBorder = DateTime.Now.Date.AddDays(1);
            while (true)
            {
                if (DateTime.Now > dayBorder)
                {
                    GenerateNewMessageDateTimes();
                    dayBorder = DateTime.Now.Date.AddDays(1);
                }

                if (ShouldSendMessage(out var partOfDay))
                {
                    await _bulkMessagingTelegramClient.SendScheduledMessageToAllUsersAsync(partOfDay);
                }
            }
        }

        private bool ShouldSendMessage(out PartOfDay category)
        {
            foreach (var dt in MessageSchedule)
            {
                if (DateTime.Now >= dt)
                {
                    MessageSchedule.Remove(dt);
                    category = DateTime.Now.PartOfDay();
                    return true;
                }
            }

            category = DateTime.Now.PartOfDay();
            return false;
        }

        private void GenerateNewMessageDateTimes()
        {
            var now = DateTime.Now;

            var partOfDay = now.PartOfDay();

            // for current part
            var start = now.TimeOfDay;
            var end = partOfDay.End;
            var restTimeRange = new TimeSpanRange(start, end);

            if (restTimeRange.Difference <= TimeSpan.FromHours(_appSettings.ThresholdInHours))
            {
                MessageSchedule.AddRange(
                    _dateTimeGenerator.GenerateDateTimesWithinRange(restTimeRange, messageCount: 1));
            }
            else
            {
                MessageSchedule.AddRange(
                    _dateTimeGenerator.GenerateDateTimesWithinRange(restTimeRange, MessageCounts[partOfDay]));
            }

            // for rest parts
            foreach (var p in partOfDay.Rest)
            {
                var range = new TimeSpanRange(p.Start, p.End);
                _dateTimeGenerator.GenerateDateTimesWithinRange(range, MessageCounts[p]);
            }
        }      
    }
}
