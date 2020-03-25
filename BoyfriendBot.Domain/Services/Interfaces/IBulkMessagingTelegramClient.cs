using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IBulkMessagingTelegramClient
    {
        Task<List<Message>> SendWakeUpMessageToAllUsersAsync();
        Task<List<Message>> SendScheduledMessageToAllUsersAsync(PartOfDay partOfDay, MessageRarity type);

    }
}
