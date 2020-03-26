using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using System.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class MonitoringManager : IMonitoringManager
    {
        private readonly IBoyfriendBotDbContextFactory _dbContextFactory;

        public MonitoringManager(IBoyfriendBotDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public bool Listening { get; set; }
        public bool SchedulingMessages { get; set; }

        public int GetTotalUsers()
        {
            using (var context = _dbContextFactory.Create())
            {
                return context.User.Select(x => x.ChatId).Count();
            }
        }
    }
}
