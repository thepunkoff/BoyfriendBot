using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.ReplyMarkups;

namespace BoyfriendBot.Domain.Services
{
    public class InlineKeyboardMenuParser : IInlineKeyboardMenuParser
    {
        private readonly InlineKeyboardMenuParserAppSettings _appSettings;

        public InlineKeyboardMenuParser(IOptions<InlineKeyboardMenuParserAppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public InlineKeyboardMenu Parse(string menuId)
        {
            var json = File.ReadAllText(_appSettings.MenusJsonRelativePath);

            var jDoc = JsonDocument.Parse(json);

            var menus = jDoc.RootElement.GetProperty("Dialogs")[0].GetProperty("Menus");

            var jMenu = menus.EnumerateArray().Where(x => x.GetProperty("Id").GetString() == menuId).FirstOrDefault();

            var jRows = jMenu.GetProperty("Markup").GetProperty("Rows");

            var rows = new List<List<InlineKeyboardButton>>();

            foreach (var jRow in jRows.EnumerateArray())
            {
                var jButtons = jRow.GetProperty("Buttons");

                var row = new List<InlineKeyboardButton>();

                foreach (var jButton in jButtons.EnumerateArray())
                {
                    var button = new InlineKeyboardButton
                    {
                        Text = jButton.GetProperty("Text").GetString(),
                        CallbackData = jButton.GetProperty("CallbackData").GetString()
                    };

                    row.Add(button);
                }

                rows.Add(row);
            }

            var menu = new InlineKeyboardMenu
            {
                Text = jMenu.GetProperty("Text").GetString(),
                ReplyMarkup = new InlineKeyboardMarkup(rows)
            };

            return menu;
        }
    }
}
