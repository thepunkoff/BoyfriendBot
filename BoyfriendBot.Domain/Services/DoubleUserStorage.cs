using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class DoubleUserStorage : IUserStorage
    {
        private readonly ILogger<DoubleUserStorage> _logger;
        private readonly ScheduledMessageServiceAppSettings _scheduledMessageServiceAppSettings;
        private readonly IBoyfriendBotDbContextFactory _dbContextFactory;
        private Dictionary<long, long> _userCache = new Dictionary<long, long>();

        public DoubleUserStorage(
              ILogger<DoubleUserStorage> logger
            ,  IOptions<ScheduledMessageServiceAppSettings> appSettings
            , IBoyfriendBotDbContextFactory dbContextFactory
            )
        {
            _logger = logger;
            _scheduledMessageServiceAppSettings = appSettings.Value;
            _dbContextFactory = dbContextFactory;

            using (var context = _dbContextFactory.Create())
            {
                var dbos = context.User.ToList();

                dbos.ForEach(x => _userCache.Add(x.UserId, x.ChatId));
            }
        }

        public int GetTotalUsers()
        {
            using (var context = _dbContextFactory.Create())
            {
                return context.User.Count();
            }
        }

        public async Task<UserDbo> GetUserByChatIdNoTracking(long chatId)
        {
            using (var context = _dbContextFactory.Create())
            {
                return await context.User
                .AsNoTracking()
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => x.ChatId == chatId)
                .FirstOrDefaultAsync();
            }
        }

        public async Task<List<UserDbo>> GetUserByChatIdRangeNoTracking(IEnumerable<long> chatIds)
        {
            using (var context = _dbContextFactory.Create())
            {
                return await context.User
                .AsNoTracking()
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => chatIds.Contains(x.ChatId))
                .ToListAsync();
            }
        }

        public async Task<List<UserDbo>> GetAllUsersForScheduledMessagesNoTracking()
        {
            using (var context = _dbContextFactory.Create())
            {
                return await context.User
                .AsNoTracking()
                .Include(x => x.UserSettings)
                .Include(x => x.RarityWeights)
                .Where(x => x.UserSettings.RecieveScheduled)
                .ToListAsync();
            }
        }

        public bool TryGetChatId(long userId, out long chatId)
        {
            _userCache.TryGetValue(userId, out chatId);

            return chatId != 0;
        }

        public bool HasUser(long userId)
        {
            return _userCache.ContainsKey(userId);
        }

        public async Task AddNewUser(long userId, long chatId)
        {
            _userCache.Add(userId, chatId);

            var userDbo = new UserDbo
            {
                UserId = userId,
                ChatId = chatId
            };

            using (var context = _dbContextFactory.Create())
            {
                context.User.Add(userDbo);

                await context.SaveChangesAsync();
            }
        }

        public async Task AddNewUser(UserDbo userDbo)
        {
            _userCache.Add(userDbo.UserId, userDbo.ChatId);

            using (var context = _dbContextFactory.Create())
            {
                userDbo.UserSettings = new UserSettingsDbo
                {
                    RecieveReminders = true,
                    RecieveScheduled = true,
                    Gender = Const.Gender.Female,
                    BotGender = Const.Gender.Male,
                    BotPersonality = Const.BotPersonality.Guy
                };

                userDbo.RarityWeights = new UserRarityWeightsDbo
                {
                    WhiteWeight = _scheduledMessageServiceAppSettings.DefaultWhiteWeight,
                    GreenWeight = _scheduledMessageServiceAppSettings.DefaultGreenWeight,
                    BlueWeight = _scheduledMessageServiceAppSettings.DefaultBlueWeight,
                    PurpleWeight = _scheduledMessageServiceAppSettings.DefaultPurpleWeight,
                    OrangeWeight = _scheduledMessageServiceAppSettings.DefaultOrangeWeight
                };

                context.User.Add(userDbo);

                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveUser(long chatId)
        {
            _userCache.Remove(chatId);

            using (var context = _dbContextFactory.Create())
            {
                var userDbo = context.User
                    .Include(x => x.UserSettings)
                    .Include(x => x.RarityWeights)
                    .Where(x => x.ChatId == chatId)
                    .FirstOrDefault();

                context.User.Remove(userDbo);

                await context.SaveChangesAsync();
            }

            _logger.LogInformation($"User removed. ChatId: {chatId}");
        }
    }
}
