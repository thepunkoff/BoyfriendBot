using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;

namespace BoyfriendBot.Domain.Services
{
    public class SessionManagerSingleton : ISessionManagerSingleton
    {
        private readonly IResourceManager _resourceManager;
        private readonly ILogger<SessionManagerSingleton> _logger;

        private Dictionary<long, List<Session>> _sessions;

        public SessionManagerSingleton(
              IResourceManager resourceManager
            , ILogger<SessionManagerSingleton> logger
            )
        {
            _resourceManager = resourceManager;
            _logger = logger;

            _sessions = new Dictionary<long, List<Session>>();
        }

        public void EndSession(long chatId, Session session)
        {
            if (_sessions.ContainsKey(chatId))
            {
                _sessions[chatId].Remove(session);

                if (_sessions[chatId].Count == 0)
                {
                    _sessions.Remove(chatId);
                }
            }
        }

        public List<Session> GetActiveSessions(long chatId)
        {
            if (_sessions.ContainsKey(chatId))
            {
                return _sessions[chatId];
            }
            else
            {
                return new List<Session>();
            }
        }

        public StateData StartSession(SessionType type, long chatId)
        {
            var session = new Session(type, chatId);

            var filePath = _resourceManager.GetSessionScriptPath(type);

            session.State.DoFile(filePath);

            AddSession(session);

            DynValue result = null;
            try
            {
                result = session.State.Call(session.State.Globals["start"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                EndSession(chatId, session);
            }

            return new StateData
            {
                Data = result
            };
        }

        public StateData UpdateSession(Session session, string userInput)
        {
            var update = session.State.Globals["update"];
            var result = session.State.Call(session.State.Globals["update"], userInput);

            return new StateData
            {
                Data = result
            };
        }

        private void AddSession(Session session)
        {
            if (!_sessions.ContainsKey(session.ChatId))
            {
                _sessions[session.ChatId] = new List<Session>();
            }

            _sessions[session.ChatId].Add(session);
        }
    }
}
