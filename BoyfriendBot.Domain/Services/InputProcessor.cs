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
        private readonly TelegramBotClient _botClient;
        private readonly IUserStorage _userStorage;
        private readonly IBotMessageProvider _botMessageProvider;
        private readonly IRarityRoller _rarityRoller;

        public InputProcessor(
              ICommandProcessor commandProcessor
            , TelegramBotClient botClient
            , IUserStorage userStorage
            , IBotMessageProvider botMessageProvider
            , IRarityRoller rarityRoller
            )
        {
            _commandProcessor = commandProcessor;
            _botClient = botClient;
            _userStorage = userStorage;
            _botMessageProvider = botMessageProvider;
            _rarityRoller = rarityRoller;
        }

        public async Task ProcessUserInput(string userInput, long chatId)
        {
            var realUser = await _userStorage.GetUserByChatIdNoTracking(chatId);

            if (userInput.StartsWith("/"))
            {
                await _commandProcessor.ProcessCommand(userInput.TrimStart('/'), chatId);
            }
            else
            {
                var responseMessage = await _botMessageProvider.GetMessage(
                    category: MessageCategory.SIMPLERESPONSE,
                    type: MessageType.STANDARD,
                    rarity: _rarityRoller.RollRarityForUser(realUser),
                    chatId);

                await _botClient.SendTextMessageAsync(chatId, responseMessage.Text);
            }
        }
    }
}
