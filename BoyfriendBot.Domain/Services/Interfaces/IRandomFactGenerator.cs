using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IRandomFactGenerator
    {
        Task<string> GenerateRandomFact();
    }
}
