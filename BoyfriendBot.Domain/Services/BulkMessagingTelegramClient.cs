using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services
{
    public class BulkMessagingTelegramClient : IBulkMessagingTelegramClient
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IBoyfriendBotDbContextFactory _dbContextFactory;
        private readonly IBotMessageProvider _messageTextProvider;
        private readonly IRarityRoller _rarityRoller;

        public BulkMessagingTelegramClient(
              ITelegramBotClientWrapper wrapper
            , IBoyfriendBotDbContextFactory dbContextFactory
            , IBotMessageProvider messageTextProvider
            , IRarityRoller rarityRoller
            )
        {
            _botClient = wrapper.Client;
            _dbContextFactory = dbContextFactory;
            _messageTextProvider = messageTextProvider;
            _rarityRoller = rarityRoller;
        }


        public async Task<List<Message>> SendWakeUpMessageToAllUsersAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var sentMessages = new List<Message>();

            IEnumerable<UserDbo> users = null;
            using (var context = _dbContextFactory.Create())
            {
                users = context.User
                    .Include(x => x.UserSettings)
                    .Include(x => x.RarityWeights)
                    .Where(x => x.UserSettings.RecieveReminders);
            }

            foreach (var user in users)
            {
                Message sentMessage = null;
                var rarity = _rarityRoller.RollRarityForUser(user);
                var message = await _messageTextProvider.GetMessage(MessageCategory.WAKEUP, MessageType.STANDARD, rarity, user.ChatId); // could already pass gender and so on

                if (message.ImageUrl == null)
                {
                    sentMessage = await _botClient.SendTextMessageAsync(user.ChatId, message.Text);
                }
                else
                {
                    sentMessage = await _botClient.SendPhotoAsync(user.ChatId, message.ImageUrl, caption: message.Text);
                }

                sentMessages.Add(sentMessage);
            }

            return sentMessages;
        }

        public async Task<List<Message>> SendScheduledMessageToAllUsersAsync(PartOfDay partOfDay, MessageRarity type, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var sentMessages = new List<Message>();

            IEnumerable<UserDbo> users = null;
            using (var context = _dbContextFactory.Create())
            {
                users = context.User
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => x.UserSettings.RecieveScheduled);
            }
            foreach (var user in users)
            {
                Message sentMessage = null;

                var rarity = _rarityRoller.RollRarityForUser(user);
                var message = await _messageTextProvider.GetMessage(Enum.Parse<MessageCategory>(partOfDay.Name.ToUpperInvariant()), MessageType.STANDARD, rarity, user.ChatId); // could already pass gender and so on

                if (message.ImageUrl == null)
                {
                    sentMessage = await _botClient.SendTextMessageAsync(user.ChatId, message.Text);
                }
                else
                {
                    sentMessage = await _botClient.SendPhotoAsync(user.ChatId, message.ImageUrl, caption: message.Text);
                }
                sentMessages.Add(sentMessage);
            }

            return sentMessages;
        }
    }
}
