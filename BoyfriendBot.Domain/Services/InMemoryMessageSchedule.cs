using AutoMapper;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    // may contain bugs, legacy class
    public class InMemoryMessageSchedule : IMessageSchedule
    {
        private readonly ILogger<InMemoryMessageSchedule> _logger;
        private Dictionary<DateTime, ScheduledMessage> _scheduledMesageCache { get; set; }

        public InMemoryMessageSchedule(
            ILogger<InMemoryMessageSchedule> logger
            , IMapper mapper
            )
        {
            _logger = logger;
            _scheduledMesageCache = new Dictionary<DateTime, ScheduledMessage>();
        }

        public async Task<List<DateTime>> GetAllScheduledMessageTimes(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            return _scheduledMesageCache.Keys.ToList();
        }

        public async Task<ScheduledMessage> GetScheduledMessage(DateTime dateTime, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            return _scheduledMesageCache[dateTime];
        }

        public async Task AddScheduledMessage(ScheduledMessage message, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _scheduledMesageCache.Add(message.Time, message);
        }

        public async Task AddScheduledMessageRange(IEnumerable<ScheduledMessage> messages, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            foreach (var message in messages)
            {
                await AddScheduledMessage(message, cancellationToken);
            }
        }

        public async Task RemoveScheduledMessage(DateTime dateTime, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _scheduledMesageCache.Remove(dateTime);
        }

        public async Task RemoveAllScheduledMessages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _scheduledMesageCache.Clear();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var messages = _scheduledMesageCache.Values.ToList();
            
            messages.Sort((x, y) =>
                x.Time > y.Time
                    ? 1
                    : x.Time < y.Time
                        ? -1
                        : 0
                    );

            foreach (var message in messages)
            {
                sb.AppendLine(message.ToString());
            }

            return Environment.NewLine + sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}
