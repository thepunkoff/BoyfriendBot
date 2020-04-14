using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services.Models
{
    public class BotMessage
    {
        public BotMessage() { }

        public string Text { get; set; }
        public InputOnlineFile Image { get; set; }

        public string Category { get; set; }
        public MessageType Type { get; set; }
        public MessageRarity Rarity { get; set; }
    }
}
