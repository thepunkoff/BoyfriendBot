using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Services.Models
{
    public class ScheduledMessage
    {
        public ScheduledMessage(MessageType type, long chatId, DateTime time)
        {
            Guid = Guid.NewGuid();
            Type = type;
            ChatId = chatId;
            Time = time;
        }

        public Guid Guid { get; set; }
        public MessageType Type { get; set; }
        public long ChatId { get; set; }
        public DateTime Time { get; set; }
    }
}
