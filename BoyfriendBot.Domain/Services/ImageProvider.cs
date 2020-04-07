using BoyfriendBot.Domain.Infrastructure.ResultProcessing;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services
{
    public class ImageProvider : IImageProvider
    {
        private readonly ILogger<ImageProvider> _logger;
        private readonly IResourceManager _resourceManager;
        private readonly IQueryImageDownloader _queryImageDownloader;

        private string ImagesDirectory { get; set; }

        public ImageProvider(
              ILogger<ImageProvider> logger
            , IQueryImageDownloader queryImageDownloader
            , IResourceManager resourceManager
            )
        {
            _logger = logger;
            _queryImageDownloader = queryImageDownloader;
            _resourceManager = resourceManager;

            ImagesDirectory = _resourceManager.GetImagesDirectory();
        }

        public Result<InputOnlineFile> GetLocalImage(ImageCategory imageCategory, string personality)
        {
            _logger.LogInformation($"Getting local image. Category: {imageCategory.ToString()}, Personality: {personality}");

            var jImages = _resourceManager.GetImagesDoc()
                .RootElement
                .GetProperty(imageCategory.ToString().ToLowerInvariant())
                .GetProperty("images")
                .EnumerateArray()
                .Where(x => x.GetProperty("personality").GetString() == personality)
                .ToList();

            var rng = new Random();
            var index = rng.Next(0, jImages.Count() - 1);
            
            var jImage = jImages[index];
            
            var fileName = jImage.GetProperty("name").GetString();
            
            var categoryDirPath = Path.Combine(ImagesDirectory, imageCategory.ToString());
            var imagePath = Path.Combine(categoryDirPath, fileName);
            
            return new BotImage(imagePath).GetImageForSending();
        }

        public async Task<Result<InputOnlineFile>> GetOnlineImage(ImageCategory imageCategory, string personality)
        {
            _logger.LogInformation($"Getting local image. Category: {imageCategory.ToString()}, Personality: {personality}");

            var query = _resourceManager.GetImagesDoc()
                .RootElement
                .GetProperty(imageCategory.ToString().ToLowerInvariant())
                .GetProperty("query")
                .GetString();

            var url = await _queryImageDownloader.GetRandomImageUrl($"{query} {personality}");

            return new BotImage(new Uri(url)).GetImageForSending();
        }
    }
}
