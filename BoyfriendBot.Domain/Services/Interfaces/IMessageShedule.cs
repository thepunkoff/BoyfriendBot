using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageSchedule
    {
        Task AddScheduledMessage(ScheduledMessage message, CancellationToken cancellationToken);
        Task AddScheduledMessageRange(IEnumerable<ScheduledMessage> messages, CancellationToken cancellationToken);
        Task RemoveScheduledMessage(ScheduledMessage message, CancellationToken cancellationToken);
        Task RemoveAllScheduledMessages(CancellationToken cancellationToken);
        Task<List<ScheduledMessage>> GetAllScheduledMessages(CancellationToken cancellationToken);
        string ToString();
    }
}
