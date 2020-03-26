using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IBulkMessagingTelegramClient
    {
        Task<List<Message>> SendWakeUpMessageToAllUsersAsync(CancellationToken cancellationToken);
        Task<List<Message>> SendScheduledMessageToAllUsersAsync(PartOfDay partOfDay, MessageRarity type, CancellationToken cancellationToken);

    }
}
