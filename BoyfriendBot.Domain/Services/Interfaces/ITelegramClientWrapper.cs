using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ITelegramClientWrapper
    {
        TelegramBotClient Client { get; set; }
    }
}
