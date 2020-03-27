﻿using BoyfriendBot.Domain.Commands;
using BoyfriendBot.Domain.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Microsoft.Extensions.DependencyInjection;
using BoyfriendBot.Domain.Core;
using System.Collections.Generic;
using System.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class CommandProcessor : ICommandProcessor
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandProcessor(
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ProcessCommand(string commandString, long chatId)
        {
            Command command = _serviceProvider.GetService<NullCommand>();

            var words = commandString.Split(" ").ToList();

            var commandId = words[0];

            words.Remove(commandId);

            var args = words.Count == 0 ? null : words.ToArray();

            if (commandId == Const.Commands.SendMenuCommand)
            {
                command = _serviceProvider.GetService<SendMenuCommand>();
            }

            await command.Execute(chatId, args);
        }
    }
}
