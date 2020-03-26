using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageSchedule
    {
        Task AddScheduledMessage(ScheduledMessage message, CancellationToken cancellationToken);
        Task AddScheduledMessageRange(IEnumerable<ScheduledMessage> messages, CancellationToken cancellationToken);
        Task RemoveScheduledMessage(DateTime dateTime, CancellationToken cancellationToken);
        Task RemoveAllScheduledMessages(CancellationToken cancellationToken);
        Task<List<DateTime>> GetAllScheduledMessageTimes(CancellationToken cancellationToken);
        Task<ScheduledMessage> GetScheduledMessage(DateTime dateTime, CancellationToken cancellationToken);
        string ToString();
    }
}
