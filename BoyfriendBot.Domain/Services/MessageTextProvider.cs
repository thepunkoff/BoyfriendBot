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
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class MessageTextProvider : IMessageTextProvider
    {
        private readonly MessageTextProviderAppSettings _appSettings;
        private readonly ILogger<MessageTextProvider> _logger;
        private readonly IMessageTextTransformer _messageTextTransformer;

        public MessageTextProvider(
              IOptions<MessageTextProviderAppSettings> appSettings
            , ILogger<MessageTextProvider> logger
            , IMessageTextTransformer messageTextTransformer
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _messageTextTransformer = messageTextTransformer;
        }

        public async Task<string> GetMessage(MessageCategory category, MessageType type, MessageRarity rarity)
        {
            var xDoc = GetXDoc();

            var categoryString = category.ToString().ToLowerInvariant();
            var typeString = type.ToString().ToLowerInvariant();
            var rarityString = rarity.ToString().ToLowerInvariant();

            var xCategory = xDoc.Root.Element(categoryString);

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

            string message = null;

            if (string.IsNullOrWhiteSpace(xMessage.Value))
            {
                _logger.LogWarning($"Message text was empty or whitespace. Category: {xCategory.Name}, Type: {typeString}, Rarity: {rarityString}");
                return null;
            }

            message = xMessage.Value;

            if (xMessage.Attribute("insert") != null && !string.IsNullOrWhiteSpace(xMessage.Attribute("insert").Value))
            {
                message = await _messageTextTransformer.ExecuteInsert(xMessage.Attribute("insert").Value, message);
            }

            if (xMessage.Attribute("rant") != null && xMessage.Attribute("rant").Value == "true")
            {
                message = _messageTextTransformer.ExecuteRant(message);
            }

            return message;
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
