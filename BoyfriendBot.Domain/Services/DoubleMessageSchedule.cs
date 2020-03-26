using AutoMapper;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    // may contain bugs, legacy class
    public class DoubleMessageSchedule : IMessageSchedule
    {
        private readonly ILogger<DoubleMessageSchedule> _logger;
        private readonly IBoyfriendBotDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;

        public DoubleMessageSchedule(
            ILogger<DoubleMessageSchedule> logger
            , IBoyfriendBotDbContextFactory dbContextFactory
            , IMapper mapper
            )
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _scheduledMesageCache = new List<ScheduledMessage>();
        }

        private List<ScheduledMessage> _scheduledMesageCache { get; set; }

        public async Task<List<ScheduledMessage>> GetAllScheduledMessages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (_scheduledMesageCache.Count != 0)
            {
                return GetCachedMessages();
            }
            else
            {
                await Cache(cancellationToken);

                return GetCachedMessages();
            }
        }

        private List<ScheduledMessage> GetCachedMessages()
        {
            var messages = new List<ScheduledMessage>();

            messages.AddRange(_scheduledMesageCache);

            return messages;
        }

        public async Task AddScheduledMessage(ScheduledMessage message, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _scheduledMesageCache.Add(message);

            var messageDbo = _mapper.Map<ScheduledMessageDbo>(message);

            using (var context = _dbContextFactory.Create())
            {
                context.MessageSchedule.Add(messageDbo);

                await context.SaveChangesAsync();
            }
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

            var messageDbo = _mapper.Map<ScheduledMessageDbo>(message);

            using (var context = _dbContextFactory.Create())
            {
                context.MessageSchedule.Add(messageDbo);

                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveAllScheduledMessages(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _scheduledMesageCache.Clear();

            using (var context = _dbContextFactory.Create())
            {
                context.MessageSchedule.FromSqlRaw("delete from \"MessageSchedule\"");

                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task Cache(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<ScheduledMessage> messages = null;
            using (var context = _dbContextFactory.Create())
            {
                var messageDbos = await context.MessageSchedule.ToListAsync(cancellationToken);

                messages = _mapper.Map<List<ScheduledMessage>>(messageDbos);
            }

            _scheduledMesageCache.AddRange(messages);
        }
    }
}
