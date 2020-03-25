using System;
using System.ComponentModel.DataAnnotations;

namespace BoyfriendBot.Domain.Data.Models
{
    public class ScheduledMessageDbo
    {
        [Key] public string Guid { get; set; }
        public string Type { get; set; }
        public long ChatId { get; set; }
        public DateTime Time { get; set; }
    }
}
