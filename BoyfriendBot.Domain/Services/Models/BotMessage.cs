using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BoyfriendBot.Domain.Services.Models
{
    public class BotMessage
    {
        public BotMessage() { }
        public BotMessage(string text, string photo)
        {
            Text = text;
            ImageUrl = photo;
        }
        public BotMessage(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
        public string ImageUrl { get; set; }
    }
}
