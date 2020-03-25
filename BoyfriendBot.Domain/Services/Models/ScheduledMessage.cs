using System;

namespace BoyfriendBot.Domain.Services.Models
{
    public class ScheduledMessage
    {
        public ScheduledMessage() { }
        public ScheduledMessage(MessageType type, MessageRarity rarity, long chatId, DateTime time)
        {
            Type = type;
            Rarity = rarity;
            ChatId = chatId;
            Time = time;
        }

        public Guid Guid { get; set; }
        public MessageType Type { get; set; }
        public MessageRarity Rarity { get; set; }
        public long ChatId { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"{Type.ToString()} - {Rarity.ToString()} - {Time.ToString()} - {ChatId}";
        }
    }
}
