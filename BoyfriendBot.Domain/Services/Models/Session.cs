using MoonSharp.Interpreter;

namespace BoyfriendBot.Domain.Services.Models
{
    public class Session
    {
        public long ChatId { get; private set; }
        public SessionType Type { get; private set; }

        public Script State { get; set; }

        public Session(SessionType type, long chatId)
        {
            ChatId = chatId;
            Type = type;
        }
    }
}
