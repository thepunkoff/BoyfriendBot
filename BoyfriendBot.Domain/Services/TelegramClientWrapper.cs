using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services
{
    public class TelegramBotClientWrapper : ITelegramBotClientWrapper
    {
        private readonly ILogger<TelegramBotClientWrapper> _logger;
        private readonly IResourceManager _recourceManager;
        public  TelegramBotClient Client { get; set; }

        public TelegramBotClientWrapper(
              ILogger<TelegramBotClientWrapper> logger
            , IResourceManager resourceManager
            )
        {
            _logger = logger;
            _recourceManager = resourceManager;

            var token = _recourceManager.GetBotToken();
            var proxy = _recourceManager.GetProxy();

            Client = new TelegramBotClient(token, proxy);
        }
    }
}
