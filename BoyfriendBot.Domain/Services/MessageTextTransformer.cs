using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rant;
using Rant.Core.ObjectModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class MessageTextTransformer : IMessageTextTransformer
    {
        private readonly ILogger<MessageTextTransformer> _logger;
        private readonly IResourceManager _resourceManager;
        private readonly IRandomFactGenerator _randomFactGenerator;

        private RantEngine _rant;

        private const string FactMark = "@fact@";

        public MessageTextTransformer(
              ILogger<MessageTextTransformer> logger
            , IRandomFactGenerator randomFactGenerator
            , IResourceManager resourceManager
            )
        {
            _randomFactGenerator = randomFactGenerator;
            _resourceManager = resourceManager;

            _logger = logger;
            _rant = new RantEngine();
            _rant.LoadPackage(_resourceManager.GetRantPackagePath());
        }

        public string ExecuteRant(string rawMessage, bool gender, bool botGender)
        {
            _rant["gender"] = gender == Const.Gender.Male
                ? new RantObject("male")
                : new RantObject("female");

            _rant["botGender"] = botGender == Const.Gender.Male
                            ? new RantObject("male")
                            : new RantObject("female");

            var rantProgram = RantProgram.CompileString(rawMessage);
            return _rant.Do(rantProgram);
        }

        public async Task<string> ExecuteInsert(string type, string markedString)
        {
            if (type == "fact")
            {
                return await InsertRandomFact(markedString);
            }
            else if (type == "joke")
            {
                return InsertRandomJoke(markedString);
            }
            else if (type == "pickup")
            {
                return InsertRandomPickup(markedString);
            }
            else
            {
                throw new Exception($"Unknown insert type: {type ?? "null"}");
            }
        }

        private async Task<string> InsertRandomFact(string rawMessageWithFactMark)
        {
            var factText = await _randomFactGenerator.GenerateRandomFact();
            var firstLetterLower = factText != null
                ? char.ToLowerInvariant(factText[0]) + factText.Substring(1)
                : Const.RedAlertMessage.ToLowerInvariant();
            if (rawMessageWithFactMark.Contains(FactMark))
            {
                var message = rawMessageWithFactMark.Replace(FactMark, firstLetterLower);
                return message;
            }
            else
            {
                _logger.LogWarning($"No FactMark was found in message. Text: \"{rawMessageWithFactMark}\"");
                return Const.RedAlertMessage;
            }
        }

        private string InsertRandomJoke(string rawMessageWithJokeMark)
        {
            throw new System.NotImplementedException();
        }

        private string InsertRandomPickup(string rawMessageWithPickuptMark)
        {
            throw new System.NotImplementedException();
        }
    }
}
