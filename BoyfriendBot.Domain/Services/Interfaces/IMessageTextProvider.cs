using BoyfriendBot.Domain.Core;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IMessageTextProvider
    {
        string GetMessage(string category);
    }
}
