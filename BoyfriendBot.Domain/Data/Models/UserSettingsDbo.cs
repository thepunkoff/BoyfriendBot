using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BoyfriendBot.Domain.Data.Models
{
    public class UserSettingsDbo
    {
        [Key]
        public long UserId { get; set; }
        public bool RecieveReminders { get; set; }
        public bool RecieveScheduled { get; set; }
    }
}
