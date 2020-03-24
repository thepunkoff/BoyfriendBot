using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ITelegramBotClientWrapper
    {
        TelegramBotClient Client { get; set; }
    }
}
