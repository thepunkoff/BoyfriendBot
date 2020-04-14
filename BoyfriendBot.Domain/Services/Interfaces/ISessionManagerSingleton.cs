using BoyfriendBot.Domain.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ISessionManagerSingleton
    {
        List<Session> GetActiveSessions(long chatId);

        Task StartSession(SessionType type, long chatId);
        void UpdateSession(Session session, string userInput);
        void EndSession(long chatId, Session session);
        void EndAllSessionsExcept(long chatId, Session sessionToKeep);
    }
}
