using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Services
{
    public class BulkMessagingTelegramClient : IBulkMessagingTelegramClient
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IBoyfriendBotDbContext _dbContext;
        private readonly IMessageTextProvider _messageTextProvider;
        private readonly IRarityRoller _rarityRoller;

        public BulkMessagingTelegramClient(
              ITelegramBotClientWrapper wrapper
            , IBoyfriendBotDbContext dbContext
            , IMessageTextProvider messageTextProvider
            , IRarityRoller rarityRoller
            )
        {
            _botClient = wrapper.Client;
            _dbContext = dbContext;
            _messageTextProvider = messageTextProvider;
            _rarityRoller = rarityRoller;
        }


        public async Task<List<Message>> SendWakeUpMessageToAllUsersAsync()
        {
            var sentMessages = new List<Message>();

            var users = _dbContext.User
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => x.UserSettings.RecieveReminders);

            foreach (var user in users)
            {
                var rarity = _rarityRoller.RollRarityForUser(user);
                var message = _messageTextProvider.GetMessage(Const.XmlAliases.WakeUpCategory, MessageType.STANDARD, rarity);

                var sentMessage = await _botClient.SendTextMessageAsync(user.ChatId, message);
                sentMessages.Add(sentMessage);
            }

            return sentMessages;
        }

        public async Task<List<Message>> SendScheduledMessageToAllUsersAsync(PartOfDay partOfDay, MessageRarity type)
        {
            var sentMessages = new List<Message>();

            var users = _dbContext.User
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => x.UserSettings.RecieveScheduled);

            foreach (var user in users)
            {
                var rarity = _rarityRoller.RollRarityForUser(user);
                var message = _messageTextProvider.GetMessage(partOfDay.Name, MessageType.STANDARD, rarity);

                var sentMessage = await _botClient.SendTextMessageAsync(user.ChatId, message);
                sentMessages.Add(sentMessage);
            }

            return sentMessages;
        }
    }
}
