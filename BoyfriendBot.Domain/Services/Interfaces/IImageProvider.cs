using BoyfriendBot.Domain.Infrastructure.ResultProcessing;
using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IImageProvider
    {
        Result<InputOnlineFile> GetLocalImage(ImageCategory imageCategory, string personality);
        Task<Result<InputOnlineFile>> GetOnlineImage(ImageCategory imageCategory, string personality);
    }
}
