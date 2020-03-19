using BoyfriendBot.Domain.Commands;
using BoyfriendBot.Domain.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Microsoft.Extensions.DependencyInjection;
using BoyfriendBot.Domain.Core;

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

        public async Task ProcessCommand(Message message)
        {
            Command command = _serviceProvider.GetService<NullCommand>();

            if (message.Text.Contains(Const.CommandPatterns.SettingsCommand))
            {
                command = _serviceProvider.GetService<ChoseSettingsCommand>();
            }

            await command.Execute(message.Chat.Id);
        }
    }
}
