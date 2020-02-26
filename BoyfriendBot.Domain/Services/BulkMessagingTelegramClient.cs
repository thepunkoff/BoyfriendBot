using BoyfriendBot.Domain.Data.Context.Interfaces;
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

        public BulkMessagingTelegramClient(
              ITelegramClientWrapper wrapper
            , IBoyfriendBotDbContext dbContext
            )
        {
            _botClient = wrapper.Client;
            _dbContext = dbContext;
        }

        public async Task<List<Message>> SendTextMessageToAllUsersAsync(string message)
        {
            var users = await _dbContext.User
                .ToListAsync();

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
