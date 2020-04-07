using BoyfriendBot.Domain.Infrastructure.ResultProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services.Models
{
    public class BotImage
    {
        private Uri _imageUrl;
        private string _localImagePath;

        public BotImage(Uri imageUrl)
        {
            _imageUrl = imageUrl;
        }
        public BotImage(string localImagePath)
        {
            _localImagePath = localImagePath;
        }

        public Result<InputOnlineFile> GetImageForSending()
        {
            if (_imageUrl != null)
            {
                var file = new InputOnlineFile(_imageUrl);
                return new Result<InputOnlineFile>(file);
            }
            else if (_localImagePath != null)
            {
                try
                {
                    using (var fileStream = new FileStream(_localImagePath, FileMode.Open))
                    {
                        var file = new InputOnlineFile(fileStream);
                        return new Result<InputOnlineFile>(file);
                    }
                }
                catch (FileNotFoundException ex)
                {
                    return new Result<InputOnlineFile>(ex.ToString());
                }
            }
            else
            {
                throw new Exception("Couldn't create image. Url or local path wasn't specified");
            }
        }
    }
}
