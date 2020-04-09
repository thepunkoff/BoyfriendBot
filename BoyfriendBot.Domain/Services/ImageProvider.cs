using BoyfriendBot.Domain.Infrastructure.ResultProcessing;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services
{
    public class ImageProvider : IImageProvider
    {
        private readonly ILogger<ImageProvider> _logger;
        private readonly IResourceManager _resourceManager;
        private readonly IQueryImageDownloader _queryImageDownloader;

        private static Dictionary<long, List<(string FileKey, FileBase File)>> _fileCache;

        static ImageProvider()
        {
            _fileCache = new Dictionary<long, List<(string FileKey, FileBase File)>>();
        }

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

        public void CacheFile(long chatId, string fileKey, FileBase file)
        {
            if (!_fileCache.ContainsKey(chatId))
            {
                _fileCache.Add(chatId, new List<(string FileKey, FileBase File)>() { (fileKey, file) });
                return;
            }

            if (!_fileCache[chatId].Any(x => x.FileKey == fileKey))
            {
                var files = _fileCache[chatId];

                files.Add((fileKey, file));
            }
        }

        public Result<InputOnlineFile> GetLocalImage(ImageCategory imageCategory, string personality, long chatId)
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

            Result<InputOnlineFile> image = null;
            if (_fileCache.ContainsKey(chatId) && _fileCache[chatId].Any(x => x.FileKey == fileName))
            {
                var fileId = _fileCache[chatId]
                    .Where(x => x.FileKey == fileName)
                    .FirstOrDefault()
                    .File.FileId;

                image = new Result<InputOnlineFile>(new InputOnlineFile(fileId));
            }
            else
            {
                var categoryDirPath = Path.Combine(ImagesDirectory, imageCategory.ToString().ToLowerInvariant());
                var imagePath = Path.Combine(categoryDirPath, fileName);

                image = new BotImage(imagePath, fileName).GetImageForSending();
            }

            return image;
        }

        public async Task<Result<InputOnlineFile>> GetOnlineImage(ImageCategory imageCategory, string personality, long chatId)
        {
            _logger.LogInformation($"Getting online image. Category: {imageCategory.ToString()}, Personality: {personality}");

            var query = _resourceManager.GetImagesDoc()
                .RootElement
                .GetProperty(imageCategory.ToString().ToLowerInvariant())
                .GetProperty("query")
                .GetString();

            var url = await _queryImageDownloader.GetRandomImageUrl($"{query} {personality}");

            Result<InputOnlineFile> image = null;
            if (_fileCache.ContainsKey(chatId) && _fileCache[chatId].Any(x => x.FileKey == url))
            {
                var fileId = _fileCache[chatId]
                    .Where(x => x.FileKey == url)
                    .FirstOrDefault()
                    .File.FileId;

                image = new Result<InputOnlineFile>(new InputOnlineFile(fileId));
            }
            else
            {
                image = new BotImage(new Uri(url), url).GetImageForSending();
            }

            return image;
        }
    }
}
