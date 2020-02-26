using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace BoyfriendBot.ConsoleApp
{
    class Program
    {
        private static ITelegramBotClient _client;
        static async Task Main(string[] args)
        {
            var proxy = new WebProxy
            {
                Address = new Uri(""),
                Credentials = new NetworkCredential
                {
                    UserName = "",
                    Password = ""
                }
            };

            _client = new TelegramBotClient("", proxy);

            var me = await _client.GetMeAsync();

            Console.WriteLine($"{me.Id} running...");

            _client.OnMessage += OnMessage;

            _client.StartReceiving();

            Thread.Sleep(int.MaxValue);
        }

        static async void OnMessage(object sender, MessageEventArgs eventArgs)
        {
            await _client.SendTextMessageAsync(eventArgs.Message.Chat, "Прости, я тебя пока что не понимаю!");
        }
    }
}
