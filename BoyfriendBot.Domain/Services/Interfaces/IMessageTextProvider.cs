using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageTextProvider
    {
        Task<string> GetMessage(MessageCategory category, MessageType type, MessageRarity rarity);
    }
}
