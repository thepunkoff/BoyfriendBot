using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rant;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class MessageTextProvider : IMessageTextProvider
    {
        private readonly MessageTextProviderAppSettings _appSettings;
        private readonly ILogger<MessageTextProvider> _logger;
        private RantEngine _rant;

        public MessageTextProvider(
              IOptions<MessageTextProviderAppSettings> appSettings
            , ILogger<MessageTextProvider> logger
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;

            _rant = new RantEngine();
            var executionPath = AppDomain.CurrentDomain.BaseDirectory;
            var rantPackagePath = Path.Combine(executionPath, _appSettings.RelativeRantPackagePath);
            _rant.LoadPackage(rantPackagePath);
        }

        public string GetMessage(string category, MessageType type, MessageRarity rarity)
        {
            var xDoc = GetXDoc();

            var xCategory = xDoc.Root.Element(category);

            var typeString = type.ToString().ToLowerInvariant();
            var rarityString = rarity.ToString().ToLowerInvariant();

            var xMessages = xCategory
                .Elements()
                .Where(x => x.Attribute(Const.XmlAliases.TypeAttribute).Value == typeString)
                .Where(x => x.Attribute(Const.XmlAliases.RarityAttribute).Value == rarityString)
                .ToList();

            if (xMessages.Count == 0)
            {
                _logger.LogWarning($"Couldn't find any message for query. Category: {xCategory.Name}, Type: {typeString}, Rarity: {rarityString}");
                return null;
            }

            var rng = new Random();

            var index = rng.Next(xMessages.Count);

            var xMessage = xMessages[index];

            if (string.IsNullOrWhiteSpace(xMessage.Value))
            {
                _logger.LogWarning($"Message text was empty or whitespace. Category: {xCategory.Name}, Type: {typeString}, Rarity: {rarityString}");
                return null;
            }

            if (xMessage.Attribute("rant") != null && xMessage.Attribute("rant").Value == "true")
            {
                var rantProgram = RantProgram.CompileString(xMessage.Value);
                return _rant.Do(rantProgram);
            }
            else
            {
                return xMessage.Value;
            }

        }

        private XDocument GetXDoc()
        {
            var executionPath = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(executionPath, _appSettings.RelativeFilePath);

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
