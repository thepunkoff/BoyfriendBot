using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services
{
    public class InputProcessor : IInputProcessor
    {
        private readonly ICommandProcessor _commandProcessor;
        private readonly ITelegramClient _telegramClient;
        private readonly IUserStorage _userStorage;
        private readonly IBotMessageProvider _botMessageProvider;
        private readonly IRarityRoller _rarityRoller;
        private readonly IStringAnalyzer _stringAnalyzer;
        private readonly ISessionManagerSingleton _sessionManagerSingleton;
        private readonly ISessionDataProcessor _sessionDataProcessor;

        public InputProcessor(
              ICommandProcessor commandProcessor
            , ITelegramClient telegramClient
            , IUserStorage userStorage
            , IBotMessageProvider botMessageProvider
            , IRarityRoller rarityRoller
            , IStringAnalyzer stringAnalyzer
            , ISessionManagerSingleton sessionManagerSingleton
            , ISessionDataProcessor sessionDataProcessor
            )
        {
            _commandProcessor = commandProcessor;
            _telegramClient = telegramClient;
            _userStorage = userStorage;
            _botMessageProvider = botMessageProvider;
            _rarityRoller = rarityRoller;
            _stringAnalyzer = stringAnalyzer;
            _sessionManagerSingleton = sessionManagerSingleton;
            _sessionDataProcessor = sessionDataProcessor;
        }

        public async Task ProcessUserInput(string userInput, long chatId)
        {
            if (_stringAnalyzer.IsMatch(userInput, MatchCategory.COMMAND))
            {
                await _commandProcessor.ProcessCommand(userInput.TrimStart('/'), chatId);
            }
            else if (_stringAnalyzer.IsMatch(userInput.ToLowerInvariant(), MatchCategory.SELFIE_REQUEST))
            {
                await _telegramClient.SendMessageAsync(
                    category: MessageCategory.SELFIE,
                    type: MessageType.STANDARD,
                    //rarity: await _rarityRoller.RollRarityForUser(chatId),
                    rarity: MessageRarity.WHITE,
                    chatId
                    );
            }
            else
            {
                var sessions = _sessionManagerSingleton.GetActiveSessions(chatId);

                if (sessions.Count() > 0)
                {
                    foreach (var session in sessions.Reverse<Session>())
                    {
                        var sessionData = session.Update(_sessionManagerSingleton, userInput);
                        await _sessionDataProcessor.ProcessAsync(sessionData, chatId);
                    }
                }
                else
                {
                    await _telegramClient.SendMessageAsync(
                        category: MessageCategory.SIMPLERESPONSE,
                        type: MessageType.STANDARD,
                        rarity: await _rarityRoller.RollRarityForUser(chatId),
                        chatId
                        );
                }
            }
        }
    }
}
