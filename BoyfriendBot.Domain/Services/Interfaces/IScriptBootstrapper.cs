using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ISessionBootstrapper
    {
        Task BootstrapSession(ISessionManagerSingleton sessionManagerSingleton, Session session);
    }
}
