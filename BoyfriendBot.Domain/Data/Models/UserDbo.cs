using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BoyfriendBot.Domain.Data.Models
{
    public class UserDbo
    {
        [Key]
        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public UserSettingsDbo UserSettings { get; set; }

        public long ChatId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
