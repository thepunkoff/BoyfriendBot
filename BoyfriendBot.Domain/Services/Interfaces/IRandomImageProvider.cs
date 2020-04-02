using System.IO;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IRandomImageProvider
    {
        Task<string> GetRandomImageUrl(string query);
    }
}
