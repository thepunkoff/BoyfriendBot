using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ISessionDataProcessor
    {
        Task ProcessAsync(StateData data, long chatId);
    }
}
