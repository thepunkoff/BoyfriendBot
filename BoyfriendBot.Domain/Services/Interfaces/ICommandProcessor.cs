using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ICommandProcessor
    {
        Task ProcessCommand(string commandString, long chatId);
    }
}
