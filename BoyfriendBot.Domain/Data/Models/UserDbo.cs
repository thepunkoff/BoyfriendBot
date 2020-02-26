using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BoyfriendBot.Domain.Data.Models
{
    public class UserDbo
    {
        [Key]
        public long ChatId { get; set; }
        public string LastKnownUsername { get; set; }
    }
}
