using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageSchedule
    {
        Task AddScheduledMessage(ScheduledMessage message);
        Task AddScheduledMessageRange(IEnumerable<ScheduledMessage> messages);
        Task RemoveScheduledMessage(ScheduledMessage message);
        Task RemoveAllScheduledMessages();
        Task<List<ScheduledMessage>> GetAllScheduledMessages();
        string ToString();
    }
}
