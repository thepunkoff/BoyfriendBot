using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
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

        public BulkMessagingTelegramClient(
              ITelegramClientWrapper wrapper
            , IBoyfriendBotDbContext dbContext
            , IMessageTextProvider messageTextProvider
            )
        {
            _botClient = wrapper.Client;
            _dbContext = dbContext;
            _messageTextProvider = messageTextProvider;
        }


        public async Task<List<Message>> SendWakeUpMessageToAllUsersAsync()
        {
            var message = _messageTextProvider.GetMessage(Const.XmlAliases.WakeUp);

            var users = _dbContext.User
                .Include(x => x.UserSettings)
                .Where(x => x.UserSettings.RecieveReminders);

            return await SendTextMessageToUsersAsync(message, users);
        }

        public async Task<List<Message>> SendScheduledMessageToAllUsersAsync(PartOfDay partOfDay)
        {
            var message = _messageTextProvider.GetMessage(partOfDay.Name);

            var users = _dbContext.User
                .Include(x => x.UserSettings)
                .Where(x => x.UserSettings.RecieveScheduled);

            return await SendTextMessageToUsersAsync(message, users);
        }

        private async Task<List<Message>> SendTextMessageToUsersAsync(string message, IQueryable<UserDbo> users)
        {
            var sentMessages = new List<Message>();
            foreach (var user in users)
            {
                var sentMessage = await _botClient.SendTextMessageAsync(user.ChatId, message);
                sentMessages.Add(sentMessage);
            }

            return sentMessages;
        }
    }
}
