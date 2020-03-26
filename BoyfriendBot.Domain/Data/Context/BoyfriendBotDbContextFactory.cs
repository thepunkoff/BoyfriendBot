using BoyfriendBot.Domain.Data.Context.Interfaces;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace BoyfriendBot.Domain.Data.Context
{
    public class BoyfriendBotDbContextFactory : IBoyfriendBotDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public BoyfriendBotDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBoyfriendBotDbContext Create()
        {
            return _serviceProvider.GetService<IBoyfriendBotDbContext>();
        }
    }
}
