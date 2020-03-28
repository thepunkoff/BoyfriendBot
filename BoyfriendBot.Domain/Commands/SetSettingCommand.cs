using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Commands
{
    public class SetSettingCommand : Command
    {
        private readonly TelegramBotClient _botClient;
        private readonly IBoyfriendBotDbContextFactory _contextFactory;
        private readonly IUserStorage _userStorage;
        private readonly ILogger<SetSettingCommand> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SetSettingCommand(
              ITelegramBotClientWrapper botClient
            , IBoyfriendBotDbContextFactory contextFactory
            , IUserStorage userStorage
            , ILogger<SetSettingCommand> logger
            , IServiceProvider serviceProvider
            )
        {
            _botClient = botClient.Client;
            _contextFactory = contextFactory;
            _userStorage = userStorage;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task Execute(long chatId, params string[] args)
        {
            var settingId = args[0];
            var settingValue = args[1];
            var user = await _userStorage.GetUserByChatIdNoTracking(chatId);
            var userId = user.UserId;

            var messageId = 0;
            if (args.Length == 3)
            {
                int.TryParse(args[1], out messageId);
            }

            var query = $"UPDATE \"UserSettings\" SET \"{settingId}\" = {settingValue} WHERE \"UserId\" = {userId}";

            using (var context = _contextFactory.Create())
            {
                try
                {
                    await ((DbContext)context).Database.ExecuteSqlRawAsync(query);

                    _logger.LogError($"Executed query \"{query}\". ChatId: {chatId}");

                    if (messageId == 0)
                    {
                        await _botClient.SendTextMessageAsync(chatId, "Готово!");

                    }
                    else
                    {
                        await _botClient.EditMessageTextAsync(chatId, messageId, "Готово!");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Query \"{query}\" wasn't executed. Exception: {ex.ToString()}");
                    await _botClient.SendTextMessageAsync(chatId, Const.ErrorMessage);
                }
            }
        }
    }
}
