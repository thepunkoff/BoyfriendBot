using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services
{
    public class SessionBootstrapper : ISessionBootstrapper
    {
        private readonly TelegramBotClient _botClient;
        private readonly IUserStorage _userStorage;
        private readonly ITelegramClient _telegramClient;
        private readonly IRarityRoller _rarityRoller;

        public SessionBootstrapper(
              ITelegramBotClientWrapper botClientWrapper
            , IUserStorage userStorage
            , ITelegramClient telegramClient
            , IRarityRoller rarityRoller
            )
        {
            _botClient = botClientWrapper.Client;
            _userStorage = userStorage;
            _telegramClient = telegramClient;
            _rarityRoller = rarityRoller;
        }

        public async Task BootstrapSession(ISessionManagerSingleton sessionManagerSingleton, Session session)
        {
            Action<string> sendMessageAsync = async (text) => await _botClient.SendTextMessageAsync(session.ChatId, text);

            Action<string> sendMessageCategory = async (categoryString) =>
                await _telegramClient.SendMessageAsync(
                    categoryString: categoryString,
                    type: MessageType.STANDARD,
                    rarity: await _rarityRoller.RollRarityForUser(session.ChatId),
                    session.ChatId
                    );

            Action<int, DynValue> delay = async (sec, callback) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(sec));

                if (callback.Type == DataType.Function)
                {
                    callback.Function.Call();
                }
            };

            Action endSession = () => sessionManagerSingleton.EndSession(session.ChatId, session);

            Action endOtherSessions = () => sessionManagerSingleton.EndAllSessionsExcept(session.ChatId, session);

            session.State = new Script();
            session.State.Options.ScriptLoader = new FileSystemScriptLoader();

            var userDbo = await _userStorage.GetUserByChatIdNoTracking(session.ChatId);

            session.State.Globals["gender"] = userDbo.UserSettings.Gender;
            session.State.Globals["bot_gender"] = userDbo.UserSettings.BotGender;
            session.State.Globals["text_message"] = sendMessageAsync;
            session.State.Globals["text_message_category"] = sendMessageCategory;
            session.State.Globals["delay"] = delay;
            session.State.Globals["end_session"] = endSession;
            session.State.Globals["end_other_sessions"] = endOtherSessions;
        }
    }
}
