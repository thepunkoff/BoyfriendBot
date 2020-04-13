using BoyfriendBot.Domain.Services.Models;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ISessionBootstrapper
    {
        void BootstrapSession(ISessionManagerSingleton sessionManagerSingleton, Session session);
    }
}
