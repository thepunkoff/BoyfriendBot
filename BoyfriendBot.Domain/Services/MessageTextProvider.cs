using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MessageTextProvider> _logger;

        public MessageTextProvider(
              IOptions<MessageTextProviderAppSettings> appSettings
            , ILogger<MessageTextProvider> logger
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public string GetMessage(string category, MessageType type)
        {
            var xDoc = GetXDoc();

            var xCategory = xDoc.Root.Element(category);

            var typeString = type.ToString().ToLowerInvariant();

            var messages = xCategory
                .Elements()
                .Where(x => x.Attribute(Const.XmlAliases.TypeAttribute).Value == typeString)
                .Select(x => x.Value)
                .ToList();

            if (messages.Count == 0)
            {
                throw new Exception("Cannot find any messages for query.");
            }

            var rng = new Random();

            var index = rng.Next(messages.Count);

            return messages[index];

        }

        private XDocument GetXDoc()
        {
            var path = Path.Combine(Environment.CurrentDirectory, _appSettings.RelativeFilePath);

            FileStream file = null;
            try
            {
                file = File.Open(path, FileMode.Open);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }

            XDocument xDoc = null;
            using (file)
            {
                xDoc = XDocument.Load(file);
            }

            return xDoc;
        }

    }
}
