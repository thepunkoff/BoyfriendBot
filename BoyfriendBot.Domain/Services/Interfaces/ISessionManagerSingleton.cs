using BoyfriendBot.Domain.Services.Models;
using System.Collections.Generic;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ISessionManagerSingleton
    {
        List<Session> GetActiveSessions(long chatId);

        StateData StartSession(SessionType type, long chatId);
        StateData UpdateSession(Session session, string userInput);
        void EndSession(long chatId, Session session);
        void EndAllSessionsExcept(long chatId, Session sessionToKeep);
    }
}
