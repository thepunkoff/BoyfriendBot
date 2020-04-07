using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IResourceManager
    {
        public string GetBotToken();
        public WebProxy GetProxy();
        public XDocument GetMessagesDoc();
        public JsonDocument GetMenusDoc();
        public XDocument GetMatchGraph();
        public string GetRantPackagePath();
        public string GetImagesDirectory();
        public JsonDocument GetImagesDoc();
    }
}
