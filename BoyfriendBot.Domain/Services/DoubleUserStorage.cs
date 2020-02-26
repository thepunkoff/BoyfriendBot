using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class DoubleUserStorage : IUserStorage
    {
        private readonly IBoyfriendBotDbContext _dbContext;
        private Dictionary<string, long> _userCache = new Dictionary<string, long>();

        public DoubleUserStorage(IBoyfriendBotDbContext dbContext)
        {
            _dbContext = dbContext;

            var dbos = _dbContext.User.ToList();

            dbos.ForEach(x => _userCache.Add(x.LastKnownUsername, x.ChatId));
        }

        public bool HasUser(string userName)
        {
            return _userCache.ContainsKey(userName);
        }

        public async Task AddNewUser(string userName, long chatId)
        {
            _userCache.Add(userName, chatId);

            var userDbo = new UserDbo
            {
                LastKnownUsername = userName,
                ChatId = chatId
            };

            _dbContext.User.Add(userDbo);

            await _dbContext.SaveChangesAsync();
        }
    }
}
