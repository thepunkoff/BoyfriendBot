using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMonitoringManager
    {
        public bool Listening { get; set; }
        public bool SchedulingMessages { get; set; }
    }
}
