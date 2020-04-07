using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
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

        public override async Task Execute(long chatId, int? messageId, params string[] args)
        {
            if (args.Length % 2 != 0)
            {
                _logger.LogError($"Odd arguments count for \"/set\" command.");
                await _botClient.SendTextMessageAsync(chatId, Const.ErrorMessage);
            }

            List<string> settingsList = new List<string>();

            for(int i = 0; i < args.Length; i += 2)
            {
                var settingId = args[i];
                var settingValue = args[i + 1];

                settingsList.Add($"{settingId} = {settingValue}");
            }

            var settings = string.Join(", ", settingsList);

            var query = $"UPDATE \"UserSettings\" SET {settings} WHERE \"ChatId\" = {chatId}";

            using (var context = _contextFactory.Create())
            {
                try
                {
                    var rowsAffected = await ((DbContext)context).Database.ExecuteSqlRawAsync(query);

                    _logger.LogInformation($"Executed query \"{query}\". ChatId: {chatId}. {rowsAffected} rows affected.");

                    await _botClient.SendTextMessageAsync(chatId, "Готово!");
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
