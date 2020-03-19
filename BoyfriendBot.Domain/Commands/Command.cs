using BoyfriendBot.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Commands
{
    public abstract class Command
    {
        public abstract long ChatId { get; protected set; }
        public abstract Task Execute(long chatId);
    }
}
