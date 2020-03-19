using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BoyfriendBot.Domain.Services
{
    public class MessageTextProvider : IMessageTextProvider
    {
        private readonly MessageTextProviderAppSettings _appSettings;

        public MessageTextProvider(IOptions<MessageTextProviderAppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string GetMessage(string category)
        {
            var xDoc = GetXDoc();

            var xCategory = xDoc.Root.Element(category);

            var messages = xCategory.Elements().Select(x => x.Value).ToList();

            var rng = new Random();

            var index = rng.Next(messages.Count);

            return messages[index];

        }

        private XDocument GetXDoc()
        {
            var path = Path.Combine(Environment.CurrentDirectory, _appSettings.RelativeFilePath);

            FileStream file = null;
            try
            {
                file = File.Open(path, FileMode.Open);
            }
            catch (FileNotFoundException ex)
            {
                // log
                throw;
            }

            XDocument xDoc = null;
            using (file)
            {
                xDoc = XDocument.Load(file);
            }

            return xDoc;
        }

    }
}
