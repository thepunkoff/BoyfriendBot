using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class ResourceManager : IResourceManager
    {
        private readonly ResourceManagerAppSettings _appSettings;
        private readonly ILogger<ResourceManager> _logger;

        private string ExecutionPath = AppDomain.CurrentDomain.BaseDirectory;

        public ResourceManager(
              IOptions<ResourceManagerAppSettings> appSettings
            , ILogger<ResourceManager> logger
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public string GetBotToken()
        {
            string token = null;
            try
            {
                var jsonText = File.ReadAllText(Path.Combine(ExecutionPath, _appSettings.BotTokenPath));
                var json = JsonDocument.Parse(jsonText);
#if RELEASE
                token = json.RootElement.GetProperty("prodToken").GetString();
#else 
                token = json.RootElement.GetProperty("testToken").GetString();
#endif
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        public XDocument GetMatchGraph()
        {
            var fullPath = Path.Combine(ExecutionPath, _appSettings.MatchGraphPath);

            try
            {
                using (var stream = File.OpenRead(fullPath))
                {
                    return XDocument.Load(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        public JsonDocument GetMenusDoc()
        {
            var fullPath = Path.Combine(ExecutionPath, _appSettings.MenusDocPath);

            try
            {
                var json = File.ReadAllText(fullPath);

                return JsonDocument.Parse(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        public XDocument GetMessagesDoc()
        {
            var fullPath = Path.Combine(ExecutionPath, _appSettings.MessagesDocPath);

            try
            {
                using (var stream = File.OpenRead(fullPath))
                {
                    return XDocument.Load(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        public WebProxy GetProxy()
        {
            var fullPath = Path.Combine(ExecutionPath, _appSettings.ProxyPath);

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

            var proxySettings = JsonConvert.DeserializeObject<ProxySettngs>(proxyJsonFile);
            var ip = proxySettings.Ip;
            var port = proxySettings.Port;
            var username = proxySettings.Username;
            var password = proxySettings.Password;

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

            return proxy;
        }

        public string GetRantPackagePath()
        {
            return _appSettings.RantPackagePath;
        }

        public string GetImagesDirectory()
        {
            return _appSettings.ImagesDirectoryAbsolutePath;
        }

        public JsonDocument GetImagesDoc()
        {
            var fullPath = Path.Combine(ExecutionPath, _appSettings.ImagesDocPath);
            string json = null;
            try
            {
                json = File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }

            var jDoc = JsonDocument.Parse(json);

            return jDoc;
        }
    }
}
