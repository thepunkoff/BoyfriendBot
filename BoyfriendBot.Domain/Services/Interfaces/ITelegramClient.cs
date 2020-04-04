using BoyfriendBot.Domain.Infrastructure.ResultProcessing;
using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ITelegramClient
    {
        Task<SendMessageResult> SendMessageAsync(MessageCategory category, MessageType type, MessageRarity rarity, long chatId);
    }
}
