using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services
{
    public class SessionDataProcessor : ISessionDataProcessor
    {
        private readonly ILogger<SessionDataProcessor> _logger;
        private readonly TelegramBotClient _botClient;
        private readonly ISessionManagerSingleton _sessionManagerSingleton;

        public SessionDataProcessor(
              ILogger<SessionDataProcessor> logger
            , ITelegramBotClientWrapper botClientWrapper
            , ISessionManagerSingleton sessionManagerSingleton
            )
        {
            _logger = logger;
            _botClient = botClientWrapper.Client;
            _sessionManagerSingleton = sessionManagerSingleton;
        }

        public async Task ProcessAsync(StateData data, long chatId)
        {
            var value = data.Data;

            if (value.Type == DataType.Boolean)
            {
                _logger.LogInformation($"[{Const.Serilog.ListeningService}] Condition wasn't met.");
            }
            else if (value.Type == DataType.String)
            {
                _logger.LogInformation($"[{Const.Serilog.ListeningService}] Script error. Message: {value.String}");
            }
            else if (value.Type == DataType.Table)
            {
                await ProcessTable(value.Table, chatId);
            }
            else
            {
                throw new Exception($"Behaviour for {value.Type.ToString()} was not defined.");
            }
        }

        private async Task ProcessTable(Table table, long chatId)
        {
            foreach (var value in table.Values)
            {
                if (value.Type == DataType.String)
                {
                    await _botClient.SendTextMessageAsync(chatId, value.String);
                }
                else if (value.Type == DataType.Number)
                {
                    await Task.Delay(TimeSpan.FromSeconds(value.Number));
                }
                else if (value.IsNil())
                {
                    RemoveSessionByTable(chatId, table);
                }
                else
                {
                    _logger.LogWarning($"[{Const.Serilog.ListeningService}] StateData table contained unexpected value type: {value.Type.ToString()}");

                    RemoveSessionByTable(chatId, table);
                }
            }
        }

        private void RemoveSessionByTable(long chatId, Table table)
        {
            var allSessions = _sessionManagerSingleton.GetActiveSessions(chatId);

            var sessionToRemove = allSessions
                .Where(x => ReferenceEquals(x.State, table.OwnerScript))
                .FirstOrDefault();

            _sessionManagerSingleton.EndSession(chatId, sessionToRemove);
        }
    }
}
