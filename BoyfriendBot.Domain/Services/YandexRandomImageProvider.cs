using BoyfriendBot.Domain.Services.Interfaces;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using System;
using System.Linq;
using System.IO;
using System.Web;

namespace BoyfriendBot.Domain.Services
{
    public class YandexRandomImageProvider : IRandomImageProvider
    {
        private HtmlParser _htmlParser;
        private Random _rng;
        private const string UrlTemplate = "https://yandex.ru/images/search?text=@";

        public YandexRandomImageProvider()
        {
            _htmlParser = new HtmlParser();
            _rng = new Random();
        }

        public async Task<string> GetRandomImageUrl(string query)
        {
            var yandexRequest = CreateYandexRequest(query);

            using (var yandexResponse = await yandexRequest.GetResponseAsync())
            using (var yandexResponseStream = yandexResponse.GetResponseStream())
            {
                var doc = await _htmlParser.ParseDocumentAsync(yandexResponseStream);

                var images = doc.QuerySelectorAll(".serp-item__link");

                var randomIndex = _rng.Next(images.Count() - 1);

                var randomImageElement = images[randomIndex];

                var imageHref = randomImageElement.GetAttribute("href");

                var imgUrlParam = "img_url=";
                var textParam = "&text";

                int from = imageHref.IndexOf(imgUrlParam) + imgUrlParam.Length;
                int to = imageHref.LastIndexOf(textParam);

                var imageUrl = imageHref.Substring(from, to - from);

                var decodedUrl = HttpUtility.UrlDecode(imageUrl);

                return decodedUrl;
            }
        }

        private HttpWebRequest CreateYandexRequest(string imageType)
        {
            var url = UrlTemplate.Replace("@", imageType);
            var request = WebRequest.CreateHttp(url);
            
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
            return request;
        }
    }
}
