using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using System.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class MonitoringManager : IMonitoringManager
    {
        public bool Listening { get; set; }
        public bool SchedulingMessages { get; set; }
    }
}
