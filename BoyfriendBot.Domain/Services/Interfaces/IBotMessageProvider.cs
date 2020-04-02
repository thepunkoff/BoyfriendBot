using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IBotMessageProvider
    {
        Task<BotMessage> GetMessage(MessageCategory category, MessageType type, MessageRarity rarity, long chatId);
    }
}
