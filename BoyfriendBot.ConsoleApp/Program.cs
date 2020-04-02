using Rant;
using Rant.Core.ObjectModel;
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
        private static RantEngine _rant = new RantEngine();
        static async Task Main(string[] args)
        {
            _rant.LoadPackage(@"F:\Downloads\Rantionary-master\Rantionary-master\build\Rantionary-3.1.0.rantpkg");
            var proxy = new WebProxy
            {
                Address = new Uri("http://194.32.239.71:56380"),
                Credentials = new NetworkCredential
                {
                    UserName = "prX0G1kz5H",
                    Password = "VXzJYfZjks"
                }
            };

            _client = new TelegramBotClient("1087063556:AAHG7tjOi-r_9auYr8vy7lFka1k1J1z9s_g", proxy);

            var me = await _client.GetMeAsync();

            Console.WriteLine($"{me.Id} running...");

            _client.OnMessage += OnMessage;

            _client.StartReceiving();

            Thread.Sleep(int.MaxValue);
        }

        static async void OnMessage(object sender, MessageEventArgs eventArgs)
        {
            var messageText = eventArgs.Message.Text;
            _rant["tense"] = new RantObject(messageText);
            var program = RantProgram.CompileString("Прости, я пока что не понимаю, что ты [if: [eq:[v: tense];present];<verb.present>;<verb.simple_past>]!");
            var message = _rant.Do(program);
            await _client.SendTextMessageAsync(eventArgs.Message.Chat, message);
        }
    }
}
