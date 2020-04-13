using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BoyfriendBot.Domain.Services
{
    public class SessionBootstrapper : ISessionBootstrapper
    {
        private readonly TelegramBotClient _botClient;

        public SessionBootstrapper(
              ITelegramBotClientWrapper botClientWrapper
            )
        {
            _botClient = botClientWrapper.Client;
        }

        public void BootstrapSession(ISessionManagerSingleton sessionManagerSingleton, Session session)
        {
            Action<string> sendMessageAsync = async (text) => await _botClient.SendTextMessageAsync(session.ChatId, text);
            session.State.Globals["text_message"] = sendMessageAsync;

            Action<int, DynValue> delay = async (sec, callback) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(sec));

                if (callback.Type == DataType.Function)
                {
                    callback.Function.Call();
                }
            };
            session.State.Globals["delay"] = delay;

            Action clearSession = () => sessionManagerSingleton.EndSession(session.ChatId, session);
            session.State.Globals["end_session"] = clearSession;

            Action endOtherSessions = () => sessionManagerSingleton.EndAllSessionsExcept(session.ChatId, session);
            session.State.Globals["end_other_sessions"] = endOtherSessions;
        }
    }
}
