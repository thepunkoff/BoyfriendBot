using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class DoubleUserStorage : IUserStorage
    {
        private readonly IBoyfriendBotDbContext _dbContext;
        private Dictionary<long, long> _userCache = new Dictionary<long, long>();

        public DoubleUserStorage(IBoyfriendBotDbContext dbContext)
        {
            _dbContext = dbContext;

            var dbos = _dbContext.User.ToList();

            dbos.ForEach(x => _userCache.Add(x.UserId, x.ChatId));
        }

        public async Task<UserDbo> GetUserByChatIdNoTracking(long chatId)
        {
            return await _dbContext.User
                .AsNoTracking()
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => x.ChatId == chatId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserDbo>> GetUserByChatIdRangeNoTracking(IEnumerable<long> chatIds)
        {
            return await _dbContext.User
                .AsNoTracking()
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => chatIds.Contains(x.ChatId))
                .ToListAsync();
        }

        public async Task<List<UserDbo>> GetAllUsersForScheduledMessagesNoTracking()
        {
            return await _dbContext.User
                .AsNoTracking()
                .Include(x => x.UserSettings)
                .Where(x => x.UserSettings.RecieveScheduled)
                .ToListAsync();
        }

        public bool TryGetChatId(long userId, out long chatId)
        {
            _userCache.TryGetValue(userId, out chatId);

            return chatId != 0;
        }

        public bool HasUser(long userId)
        {
            return _userCache.ContainsKey(userId);
        }

        public async Task AddNewUser(long userId, long chatId)
        {
            _userCache.Add(userId, chatId);

            var userDbo = new UserDbo
            {
                UserId = userId,
                ChatId = chatId
            };

            _dbContext.User.Add(userDbo);

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddNewUser(UserDbo userDbo)
        {
            _userCache.Add(userDbo.UserId, userDbo.ChatId);

            _dbContext.User.Add(userDbo);

            await _dbContext.SaveChangesAsync();
        }
    }
}
