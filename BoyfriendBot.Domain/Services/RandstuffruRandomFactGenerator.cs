using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class RandstuffruRandomFactGenerator : IRandomFactGenerator
    {
        private readonly ILogger<RandstuffruRandomFactGenerator> _logger;

        private const string RandstuffEndpointUrl = "https://randstuff.ru/fact/generate/";

        public RandstuffruRandomFactGenerator(
              ILogger<RandstuffruRandomFactGenerator> logger
            )
        {
            _logger = logger;
        }

        public async Task<string> GenerateRandomFact()
        {
            var request = CreateRequest();
            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    _logger.LogInformation("Random fact request to randstuff.ru sent.");

                    using (var jsonStream = response.GetResponseStream())
                    {
                        var doc = await JsonDocument.ParseAsync(jsonStream);

                        var factText = doc.RootElement.GetProperty("fact").GetProperty("text").GetString();

                        return factText;
                    }
                }
            }
            catch (WebException ex)
            {
                _logger.LogError($"[{((HttpWebResponse)ex.Response).StatusCode}] {ex.ToString()}");
                return null;
            }
        }

        private HttpWebRequest CreateRequest()
        {
            var request = WebRequest.CreateHttp(RandstuffEndpointUrl);

            request.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-GB,en;q=0.9,en-US;q=0.8,ru;q=0.7");
            request.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            request.Headers.Add(HttpRequestHeader.Host, "randstuff.ru");
            request.Headers.Add("Origin", "https://randstuff.ru");
            request.Headers.Add("Referer", "https://randstuff.ru/fact/");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            return request;
        }
    }
}
