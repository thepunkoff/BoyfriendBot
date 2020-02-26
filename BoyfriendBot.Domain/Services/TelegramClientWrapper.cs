using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Telegram.Bot;

namespace BoyfriendBot.Domain.Services
{
    public class TelegramClientWrapper : ITelegramClientWrapper
    {
        private IConfiguration Configuration { get; set; }
        public  TelegramBotClient Client { get; set; }

        public TelegramClientWrapper(
            IConfiguration configuration
            )
        {
            Configuration = configuration;

            // TODO get proxy from file
            var proxy = new WebProxy
            {
                Address = new Uri(""),
                Credentials = new NetworkCredential
                {
                    UserName = "",
                    Password = ""
                }
            };

            var token = File.ReadAllText(Configuration.GetValue<string>("BotTokenRelativeFilePath"));

            Client = new TelegramBotClient(token, proxy);
        }
    }
}
