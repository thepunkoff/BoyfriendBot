using System;

namespace BoyfriendBot.Domain.Services.Models
{
    public class ScheduledMessage
    {
        public ScheduledMessage() { }
        public ScheduledMessage(MessageCategory category, MessageType type, MessageRarity rarity, long chatId, DateTime time)
        {
            Type = type;
            Category = category;
            Rarity = rarity;
            ChatId = chatId;
            Time = time;
        }

        public Guid Guid { get; set; }
        public MessageType Type { get; set; }
        public MessageCategory Category { get; set; }
        public MessageRarity Rarity { get; set; }
        public long ChatId { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"{Category.ToString()} - {Type.ToString()} - {Rarity.ToString()} - {Time.ToString()} - {ChatId}";
        }
    }
}
