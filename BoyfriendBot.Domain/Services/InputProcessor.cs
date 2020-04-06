using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
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

        public InputProcessor(
              ICommandProcessor commandProcessor
            , ITelegramClient telegramClient
            , IUserStorage userStorage
            , IBotMessageProvider botMessageProvider
            , IRarityRoller rarityRoller
            , IStringAnalyzer stringAnalyzer
            )
        {
            _commandProcessor = commandProcessor;
            _telegramClient = telegramClient;
            _userStorage = userStorage;
            _botMessageProvider = botMessageProvider;
            _rarityRoller = rarityRoller;
            _stringAnalyzer = stringAnalyzer;
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
                    category: MessageCategory.ANY,
                    type: MessageType.STANDARD,
                    //rarity: await _rarityRoller.RollRarityForUser(chatId),
                    rarity: MessageRarity.BLUE,
                    chatId
                    );
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
