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

        public async Task<List<ScheduledMessage>> GetAllScheduledMessages()
        {
            return GetCachedMessages();
        }

        private List<ScheduledMessage> GetCachedMessages()
        {
            var messages = new List<ScheduledMessage>();

            messages.AddRange(_scheduledMesageCache);

            return messages;
        }

        public async Task AddScheduledMessage(ScheduledMessage message)
        {
            _scheduledMesageCache.Add(message);
        }

        public async Task AddScheduledMessageRange(IEnumerable<ScheduledMessage> messages)
        {
            foreach (var message in messages)
            {
                await AddScheduledMessage(message);
            }
        }

        public async Task RemoveScheduledMessage(ScheduledMessage message)
        {
            _scheduledMesageCache.Remove(message);
        }

        public async Task RemoveAllScheduledMessages()
        {
            _scheduledMesageCache.Clear();
        }
    }
}
