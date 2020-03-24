using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageTextProvider
    {
        string GetMessage(string category, MessageType type);
    }
}
