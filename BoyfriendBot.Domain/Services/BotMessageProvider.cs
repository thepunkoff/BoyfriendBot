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
        private readonly IImageProvider _imageProvider;
        private readonly IResourceManager _resourceManager;

        public BotMessageProvider(
              IOptions<MessageTextProviderAppSettings> appSettings
            , ILogger<BotMessageProvider> logger
            , IMessageTextTransformer messageTextTransformer
            , IUserStorage userStorage
            , IImageProvider imageProvider
            , IResourceManager resourceManager
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _messageTextTransformer = messageTextTransformer;
            _userStorage = userStorage;
            _imageProvider = imageProvider;
            _resourceManager = resourceManager;
        }

        public async Task<BotMessage> GetMessage(MessageCategory category, MessageType type, MessageRarity rarity, long chatId)
        {
            var xDoc = _resourceManager.GetMessagesDoc();

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
            var botPersonality = user.UserSettings.BotPersonality;

            message.Text = xMessage.Value;

            if (xMessage.Attribute("insert") != null && !string.IsNullOrWhiteSpace(xMessage.Attribute("insert").Value))
            {
                message.Text = await _messageTextTransformer.ExecuteInsert(xMessage.Attribute("insert").Value, message.Text);
            }

            if (xMessage.Attribute("rant") != null && xMessage.Attribute("rant").Value == "true")
            {
                message.Text = _messageTextTransformer.ExecuteRant(message.Text, gender, botGender);
            }

            if (xMessage.Attribute("image") != null && !string.IsNullOrWhiteSpace(xMessage.Attribute("image").Value))
            {
                var imageCategory = Enum.Parse<ImageCategory>(xMessage.Attribute("image").Value.ToUpperInvariant());
                var localResult = _imageProvider.GetLocalImage(imageCategory, botPersonality, chatId);

                if (localResult.Value != null)
                {
                    message.Image = localResult.Value;
                }
                else
                {
                    _logger.LogError($"Couldn't get local image. Reason: {localResult.Message}");

                    var onlineResult = await _imageProvider.GetOnlineImage(imageCategory, botPersonality, chatId);

                    if (onlineResult.Value != null)
                    {
                        message.Image = onlineResult.Value;
                    }
                    else
                    {
                        _logger.LogError($"Couldn't get online image. Reason: {onlineResult.Message}");
                    }
                }
            }

            return message;
        }
    }
}
