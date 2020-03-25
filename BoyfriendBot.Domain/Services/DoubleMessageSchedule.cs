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
    public class DoubleMessageSchedule : IMessageSchedule
    {
        private readonly ILogger<DoubleMessageSchedule> _logger;
        private readonly IBoyfriendBotDbContext _dbContext;
        private readonly IMapper _mapper;

        public DoubleMessageSchedule(
            ILogger<DoubleMessageSchedule> logger
            , IBoyfriendBotDbContext dbContext
            , IMapper mapper
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
            _scheduledMesageCache = new List<ScheduledMessage>();
        }

        private List<ScheduledMessage> _scheduledMesageCache { get; set; }

        public async Task<List<ScheduledMessage>> GetAllScheduledMessages()
        {
            if (_scheduledMesageCache.Count != 0)
            {
                return GetCachedMessages();
            }
            else
            {
                await Cache();

                return GetCachedMessages();
            }
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

            var messageDbo = _mapper.Map<ScheduledMessageDbo>(message);

            _dbContext.MessageSchedule.Add(messageDbo);

            await _dbContext.SaveChangesAsync();
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

            var messageDbo = _mapper.Map<ScheduledMessageDbo>(message);

            _dbContext.MessageSchedule.Remove(messageDbo);

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveAllScheduledMessages()
        {
            _scheduledMesageCache.Clear();

            _dbContext.MessageSchedule.FromSqlRaw("delete from \"MessageSchedule\"");

            await _dbContext.SaveChangesAsync();
        }

        private async Task Cache()
        {
            var messageDbos = await _dbContext.MessageSchedule.ToListAsync();

            var messages = _mapper.Map<List<ScheduledMessage>>(messageDbos);

            _scheduledMesageCache.AddRange(messages);
        }
    }
}
