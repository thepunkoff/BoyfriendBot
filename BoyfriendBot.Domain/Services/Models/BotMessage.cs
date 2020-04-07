using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Telegram.Bot.Types.InputFiles;

namespace BoyfriendBot.Domain.Services.Models
{
    public class BotMessage
    {
        public BotMessage() { }
        public BotMessage(string text, InputOnlineFile image)
        {
            Text = text;
            Image = image;
        }
        public BotMessage(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
        public InputOnlineFile Image { get; set; }
    }
}
