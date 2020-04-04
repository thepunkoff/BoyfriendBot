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
    public class BotMessageProvider : IBotMessageProvider
    {
        private readonly MessageTextProviderAppSettings _appSettings;
        private readonly ILogger<BotMessageProvider> _logger;
        private readonly IMessageTextTransformer _messageTextTransformer;
        private readonly IUserStorage _userStorage;
        private readonly IRandomImageProvider _randomImageProvider;

        public BotMessageProvider(
              IOptions<MessageTextProviderAppSettings> appSettings
            , ILogger<BotMessageProvider> logger
            , IMessageTextTransformer messageTextTransformer
            , IUserStorage userStorage
            , IRandomImageProvider randomImageProvider
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _messageTextTransformer = messageTextTransformer;
            _userStorage = userStorage;
            _randomImageProvider = randomImageProvider;
        }

        public async Task<BotMessage> GetMessage(MessageCategory category, MessageType type, MessageRarity rarity, long chatId)
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

            BotMessage message = new BotMessage();

            if (string.IsNullOrWhiteSpace(xMessage.Value))
            {
                _logger.LogWarning($"Message text was empty or whitespace. Category: {xCategory.Name}, Type: {typeString}, Rarity: {rarityString}");
                return null;
            }

            var user = await _userStorage.GetUserByChatIdNoTracking(chatId);

            var gender = user.UserSettings.Gender;
            var botGender = user.UserSettings.BotGender;

            message.Text = xMessage.Value;

            if (xMessage.Attribute("insert") != null && !string.IsNullOrWhiteSpace(xMessage.Attribute("insert").Value))
            {
                message.Text = await _messageTextTransformer.ExecuteInsert(xMessage.Attribute("insert").Value, message.Text);
            }

            var rantOn = false;
            if (xMessage.Attribute("rant") != null && xMessage.Attribute("rant").Value == "true")
            {
                rantOn = true;
                message.Text = _messageTextTransformer.ExecuteRant(message.Text, gender, botGender);
            }

            if (xMessage.Attribute("imagesrc") != null && !string.IsNullOrWhiteSpace(xMessage.Attribute("imagesrc").Value))
            {

            }

            if (xMessage.Attribute("image") != null && !string.IsNullOrWhiteSpace(xMessage.Attribute("image").Value))
            {
                message.ImageUrl = await _randomImageProvider.GetRandomImageUrl(xMessage.Attribute("image").Value);
            }

            if (xMessage.Attribute("imagerant") != null && !string.IsNullOrWhiteSpace(xMessage.Attribute("imagerant").Value))
            {
                var searchQuery = _messageTextTransformer.ExecuteRant(xMessage.Attribute("imagerant").Value, gender, botGender);
                message.ImageUrl = await _randomImageProvider.GetRandomImageUrl(searchQuery);
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
