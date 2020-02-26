using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class MessageTextProvider : IMessageTextProvider
    {
        private readonly MessageTextProviderAppSettings _appSettings;

        public MessageTextProvider(IOptions<MessageTextProviderAppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string GetMessage(PartOfDay partOfDay)
        {
            var path = Path.Combine(Environment.CurrentDirectory, _appSettings.RelativeFilePath);
            using var file = File.Open(path, FileMode.Open);

            var xDoc = XDocument.Load(file);

            var xPart = xDoc.Root.Element(partOfDay.Name);

            var messages = xPart.Elements().Select(x => x.Value).ToList();

            var rng = new Random();

            var index = rng.Next(messages.Count);

            return messages[index];
        }
    }
}
