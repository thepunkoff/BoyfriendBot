using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoyfriendBot.Domain.Services
{
    public class MonitoringManager : IMonitoringManager
    {
        private readonly IBoyfriendBotDbContext _dbContext;

        public MonitoringManager(IBoyfriendBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool Listening { get; set; }
        public bool SchedulingMessages { get; set; }

        public int GetTotalUsers()
        {
            return _dbContext.User.Select(x => x.ChatId).Count();
        }
    }
}
