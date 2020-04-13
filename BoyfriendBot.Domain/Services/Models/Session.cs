using BoyfriendBot.Domain.Services.Interfaces;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace BoyfriendBot.Domain.Services.Models
{
    public class Session
    {
        public long ChatId { get; private set; }
        public SessionType Type { get; private set; }

        public Script State { get; private set; }

        public Session(SessionType type, long chatId)
        {
            ChatId = chatId;
            Type = type;
            State = new Script();
            State.Options.ScriptLoader = new FileSystemScriptLoader();
        }

        public StateData Update(ISessionManagerSingleton sessionManager, string userInput)
        {
            return sessionManager.UpdateSession(this, userInput);
        }
    }
}
