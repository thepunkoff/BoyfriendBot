using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class InputProcessor : IInputProcessor
    {
        private readonly ICommandProcessor _commandProcessor;
        private readonly ITelegramClient _telegramClient;
        private readonly IRarityRoller _rarityRoller;
        private readonly IStringAnalyzer _stringAnalyzer;
        private readonly ISessionManagerSingleton _sessionManagerSingleton;

        public InputProcessor(
              ICommandProcessor commandProcessor
            , ITelegramClient telegramClient
            , IRarityRoller rarityRoller
            , IStringAnalyzer stringAnalyzer
            , ISessionManagerSingleton sessionManagerSingleton
            )
        {
            _commandProcessor = commandProcessor;
            _telegramClient = telegramClient;
            _rarityRoller = rarityRoller;
            _stringAnalyzer = stringAnalyzer;
            _sessionManagerSingleton = sessionManagerSingleton;
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
            else if (_stringAnalyzer.IsMatch(userInput.ToLowerInvariant(), MatchCategory.OFFENDED_SESSION_START))
            {
                _sessionManagerSingleton.StartSession(SessionType.OFFENDED, chatId);
            }
            else
            {
                var sessions = _sessionManagerSingleton.GetActiveSessions(chatId);

                if (sessions.Count() > 0)
                {
                    foreach (var session in sessions.Reverse<Session>())
                    {
                        _sessionManagerSingleton.UpdateSession(session, userInput);
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
