using System.IO;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IQueryImageDownloader
    {
        Task<string> GetRandomImageUrl(string query);
    }
}
