﻿using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects.Deserializers;

namespace YuGiOhV2.Services
{
    public class Web
    {

        private HttpClient _http;
        private HtmlParser _parser;

        private const string PricesBaseUrl = "http://yugiohprices.com/api/get_card_prices/";
        private const string ImagesBaseUrl = "http://yugiohprices.com/api/card_image/";

        public Web()
        {

            _http = new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                Proxy = null
            });

            _parser = new HtmlParser();

        }

        public async Task<IHtmlDocument> GetDom(string url)
        {

            var response = await Check(url).ConfigureAwait(false);
            var html = await response.ReadAsStringAsync().ConfigureAwait(false);

            return await _parser.ParseAsync(html);

        }

        public async Task Post(string url, string content, string authorization = null)
        {

            var payload = new StringContent(content);

            //if only httpclient had a way to easily set seperate authentication headers or global with thread safety :/
            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = payload
            };

            if(!string.IsNullOrEmpty(authorization))
                message.Headers.Authorization = new AuthenticationHeaderValue(authorization);

            await _http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);

        }

        public async Task<YuGiOhPrices> GetPrices(string name, string realName = null)
        {

            var response = await GetDeserializedContent<YuGiOhPrices>($"{PricesBaseUrl}{Uri.EscapeUriString(name)}").ConfigureAwait(false);

            if ((response == null || response.Status == "fail") && !string.IsNullOrEmpty(realName))
                response = await GetDeserializedContent<YuGiOhPrices>($"{PricesBaseUrl}{Uri.EscapeUriString(realName)}");

            return response;

        }

        public async Task<Stream> GetStream(string url)
        {

            using (var response = await Check(url).ConfigureAwait(false))
            using (var stream = await response.ReadAsStreamAsync().ConfigureAwait(false))
            {

                var copy = new MemoryStream();

                await stream.CopyToAsync(copy).ConfigureAwait(false);
                copy.Seek(0, SeekOrigin.Begin);

                return copy;

            }

        }

        public async Task<T> GetDeserializedContent<T>(string url)
        {

            var response = await Check(url).ConfigureAwait(false);
            var json = await response.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(json);

        }

        private async Task<HttpContent> Check(string url)
        {

            HttpResponseMessage response;

            do
            {

                response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            } while (!response.IsSuccessStatusCode);

            return response.Content;

        }

    }
}
