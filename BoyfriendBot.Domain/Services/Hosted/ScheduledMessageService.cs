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
        private readonly IMessageTextProvider _messageTextProvider;
        private readonly IBulkMessagingTelegramClient _bulkMessagingTelegramClient;
        private readonly ILogger<ScheduledMessageService> _logger;
        private readonly IMonitoringManager _monitoringManager;

        private Dictionary<PartOfDay, int> MessageCounts { get; set; }

        private List<DateTime> MessageSchedule { get; set; }

        public ScheduledMessageService(
              IOptions<ScheduledMessageServiceAppSettings> appSettings
            , IMessageTextProvider messageTextProvider
            , IBulkMessagingTelegramClient bulkMessagingTelegramClient
            , ILogger<ScheduledMessageService> logger
            , IMonitoringManager monitoringManager
            )
        {
            _appSettings = appSettings.Value;
            _messageTextProvider = messageTextProvider;
            _bulkMessagingTelegramClient = bulkMessagingTelegramClient;
            _logger = logger;
            _monitoringManager = monitoringManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_appSettings.ScheduledMessageServiceOn)
            {
                return Task.CompletedTask;
            }

            _logger.LogInformation("Initializing scheduled messaging service...");

            InitializeCountDictionary();
            GenerateNewMessageDateTimes();

            _logger.LogInformation("Messages scheduled:");
            foreach (var dt in MessageSchedule)
            {
                _logger.LogInformation($"{dt.PartOfDay().Name} - {dt}");
            }

            _logger.LogInformation("Started");

            var task = Task.Run(Run);

            return Task.CompletedTask;
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _monitoringManager.SchedulingMessages = false;

            _logger.LogInformation("Stopped");
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
                    var message = _messageTextProvider.GetMessage(partOfDay);

                    await _bulkMessagingTelegramClient.SendTextMessageToAllUsersAsync(message);
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

        private void InitializeCountDictionary()
        {
            MessageCounts = new Dictionary<PartOfDay, int>
            {
                [PartOfDay.Night] = _appSettings.NightMessagesCount,
                [PartOfDay.Morning] = _appSettings.MorningMessagesCount,
                [PartOfDay.Afternoon] = _appSettings.AfternoonMessagesCount,
                [PartOfDay.Evening] = _appSettings.EveningMessagesCount
            };
        }

        private void GenerateNewMessageDateTimes()
        {
            MessageSchedule = new List<DateTime>();

            var partOfDay = DateTime.Now.PartOfDay();

            // for current part
            var start = DateTime.Now.TimeOfDay;
            var end = partOfDay.End;
            var restTimeRange = new TimeSpanRange(start, end);

            if (restTimeRange.Difference <= TimeSpan.FromHours(_appSettings.ThresholdInHours))
            {
                GenerateMessagesDateTimesForPart(partOfDay, restTimeRange, oneMessage: true);
            }
            else
            {
                GenerateMessagesDateTimesForPart(partOfDay, restTimeRange);
            }

            // for rest parts
            foreach (var p in partOfDay.Rest)
            {
                var range = new TimeSpanRange(p.Start, p.End);
                GenerateMessagesDateTimesForPart(p, range);
            }
        }

        private void GenerateMessagesDateTimesForPart(PartOfDay partOfDay, TimeSpanRange range, bool oneMessage = false)
        {
            var count = oneMessage ? 1: MessageCounts[partOfDay];
            
            var rng = new Random();
           
            for (int i = 0; i < count; i++)
            {
                var day = default(DateTime);
                if (partOfDay.Name == Const.PartOfDay.Night)
                {
                    day = DateTime.Now.Date.AddDays(1);
                }
                else
                {
                    day = DateTime.Now.Date;
                }

                var maxSeconds = (int)range.End.Subtract(range.Start).TotalSeconds;

                var randomDateTime = day.Add(range.Start).AddSeconds(rng.Next(maxSeconds));
                MessageSchedule.Add(randomDateTime);
            }
        }        
    }
}
