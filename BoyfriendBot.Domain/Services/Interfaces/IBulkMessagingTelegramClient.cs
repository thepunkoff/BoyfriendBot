using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IBulkMessagingTelegramClient
    {
        Task<List<Message>> SendTextMessageToAllUsersAsync(string message);
    }
}
