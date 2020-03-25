using BoyfriendBot.Domain.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IUserStorage
    {
        bool TryGetChatId(long userId, out long chatId);
        bool HasUser(long userId);

        Task<UserDbo> GetUserByChatIdNoTracking(long chatId);
        Task<List<UserDbo>> GetUserByChatIdRangeNoTracking(IEnumerable<long> chatIds);
        Task<List<UserDbo>> GetAllUsersForScheduledMessagesNoTracking();
        Task AddNewUser(long userId, long chatId);
        Task AddNewUser(UserDbo userDbo);
    }
}
