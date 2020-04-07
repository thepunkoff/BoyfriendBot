using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Commands
{
    public abstract class Command
    {
        public abstract Task Execute(long chatId, int? messageId, params string[] args);
    }
}
