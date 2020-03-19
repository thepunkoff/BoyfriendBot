using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Commands
{
    public class NullCommand : Command
    {
        public override long ChatId { get; protected set; }

        public override Task Execute(long chatId)
        {
            return Task.CompletedTask;
        }
    }
}
