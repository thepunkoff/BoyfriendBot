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
        private readonly ILogger<DoubleMessageSchedule> _logger;

        public InMemoryMessageSchedule(
            ILogger<DoubleMessageSchedule> logger
            , IMapper mapper
            )
        {
            _logger = logger;
            _scheduledMesageCache = new List<ScheduledMessage>();
        }

        private List<ScheduledMessage> _scheduledMesageCache { get; set; }

        public async Task<List<ScheduledMessage>> GetAllScheduledMessages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            return _scheduledMesageCache;
        }

        public async Task AddScheduledMessage(ScheduledMessage message, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _scheduledMesageCache.Add(message);
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

        public async Task RemoveScheduledMessage(ScheduledMessage message, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _scheduledMesageCache.Remove(message);
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

            foreach (var message in _scheduledMesageCache)
            {
                sb.AppendLine(message.ToString());
            }

            return Environment.NewLine + sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}
