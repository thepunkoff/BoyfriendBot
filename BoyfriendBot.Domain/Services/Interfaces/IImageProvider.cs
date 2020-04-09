using BoyfriendBot.Domain.Infrastructure.ResultProcessing;
using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IImageProvider
    {
        public void CacheFile(long chatId, string fileName, FileBase file);
        Result<InputOnlineFile> GetLocalImage(ImageCategory imageCategory, string personality, long chatId);
        Task<Result<InputOnlineFile>> GetOnlineImage(ImageCategory imageCategory, string personality, long chatId);
    }
}
