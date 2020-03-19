using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoyfriendBot.WebApp.Models
{
    public class MonitorViewModel
    {
        public string Description { get; set; }
        public TimeSpan Uptime { get; set; }
        public bool Listening { get; set; }
        public bool SchedulingMessages { get; set; }
        public int TotalUsers { get; set; }
    }
}
