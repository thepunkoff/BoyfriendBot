using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BoyfriendBot.Domain.Data.Models
{
    public class UserSettingsDbo
    {
        [Key]
        public long ChatId { get; set; }

        [ForeignKey("ChatId")]
        public UserDbo User { get; set; }
        public bool Gender { get; set; }
        public bool BotGender { get; set; }
        public bool RecieveReminders { get; set; }
        public bool RecieveScheduled { get; set; }
    }
}
