using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Commands
{
    public class NullCommand : Command
    {
        public override async Task Execute(long chatId, int? messageId, params string[] args)
        {
            return;
        }
    }
}
