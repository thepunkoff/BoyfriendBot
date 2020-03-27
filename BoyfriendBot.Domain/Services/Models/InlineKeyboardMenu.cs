using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace BoyfriendBot.Domain.Services.Models
{
    public class InlineKeyboardMenu
    {
        public string Text { get; set; }
        public InlineKeyboardMarkup ReplyMarkup { get; set; }
    }
}
