using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services
{
    public class TelegramBotClientWrapper : ITelegramBotClientWrapper
    {
        private readonly ILogger<TelegramBotClientWrapper> _logger;
        private IConfiguration Configuration { get; set; }
        public  TelegramBotClient Client { get; set; }

        public TelegramBotClientWrapper(
              IConfiguration configuration
            , ILogger<TelegramBotClientWrapper> logger
            )
        {
            _logger = logger;

            Configuration = configuration;

            var proxyAppSettings = GetProxyAppSettings();

            var ip = proxyAppSettings.Ip;
            var port = proxyAppSettings.Port;
            var username = proxyAppSettings.Username;
            var password = proxyAppSettings.Password;

            var useDefaultCredentials = string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password);

            var proxy = new WebProxy
            {
                Address = new Uri($"http://{ip}:{port}"),
                UseDefaultCredentials = useDefaultCredentials
            };

            // wtf? inline не срабатывало
            proxy.Credentials = new NetworkCredential()
            {
                UserName = username,
                Password = password
            };

            var executionPath = AppDomain.CurrentDomain.BaseDirectory;
            var tokenFile = Configuration.GetValue<string>("BotTokenRelativePath");

            string token = null;
            try
            {
                token = File.ReadAllText(Path.Combine(executionPath, tokenFile));
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }

            Client = new TelegramBotClient(token, proxy);
        }


        private ProxyAppSettngs GetProxyAppSettings()
        {
            var executionPath = AppDomain.CurrentDomain.BaseDirectory;
            var proxyJsonPath = Configuration.GetValue<string>("ProxyConfigRelativePath");
            var fullPath = Path.Combine(executionPath, proxyJsonPath);

            string proxyJsonFile = null;
            try
            {
                proxyJsonFile = File.ReadAllText(fullPath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }

            var proxyAppSettings = JsonConvert.DeserializeObject<ProxyAppSettngs>(proxyJsonFile);

            return proxyAppSettings;
        }
    }
}
