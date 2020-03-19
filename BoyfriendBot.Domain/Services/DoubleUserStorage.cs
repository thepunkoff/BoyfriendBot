﻿using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class DoubleUserStorage : IUserStorage
    {
        private readonly IBoyfriendBotDbContext _dbContext;
        private Dictionary<long, long> _userCache = new Dictionary<long, long>();

        public DoubleUserStorage(IBoyfriendBotDbContext dbContext)
        {
            _dbContext = dbContext;

            var dbos = _dbContext.User.ToList();

            dbos.ForEach(x => _userCache.Add(x.UserId, x.ChatId));
        }

        public bool HasUser(long userId)
        {
            return _userCache.ContainsKey(userId);
        }

        public async Task AddNewUser(long userId, long chatId)
        {
            _userCache.Add(userId, chatId);

            var userDbo = new UserDbo
            {
                UserId = userId,
                ChatId = chatId
            };

            _dbContext.User.Add(userDbo);

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddNewUser(UserDbo userDbo)
        {
            _userCache.Add(userDbo.UserId, userDbo.ChatId);

            _dbContext.User.Add(userDbo);

            await _dbContext.SaveChangesAsync();
        }
    }
}
