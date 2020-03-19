using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services
{
    public class TelegramClientWrapper : ITelegramClientWrapper
    {
        private IConfiguration Configuration { get; set; }
        public  TelegramBotClient Client { get; set; }

        public TelegramClientWrapper(
              IConfiguration configuration
            )
        {
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
                Credentials = new NetworkCredential
                {
                    UserName = username,
                    Password = password
                },
                UseDefaultCredentials = useDefaultCredentials
            };

            var tokenFile = Configuration.GetValue<string>("BotTokenRelativePath");

            string token = null;
            try
            {
                token = File.ReadAllText(tokenFile);
            }
            catch (FileNotFoundException ex)
            {
                // log
                throw;
            }

            Client = new TelegramBotClient(token, proxy);
        }

        private ProxyAppSettngs GetProxyAppSettings()
        {
            var proxyJsonPath = Configuration.GetValue<string>("ProxyConfigRelativePath");
            var fullPath = Path.Combine(Environment.CurrentDirectory, proxyJsonPath);

            string proxyJsonFile = null;
            try
            {
                proxyJsonFile = File.ReadAllText(fullPath);
            }
            catch (FileNotFoundException ex)
            {
                // log
                throw;
            }

            var proxyAppSettings = JsonConvert.DeserializeObject<ProxyAppSettngs>(proxyJsonFile);

            return proxyAppSettings;
        }
    }
}
