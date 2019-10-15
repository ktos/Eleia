using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Eleia.CoyoteApi
{
    public class CoyoteHandler
    {
        private HttpClient hc;
        private ILogger _logger;
        private CookieContainer cookieContainer;
        private string csrfToken;

        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(5);

        public CoyoteHandler(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger("CoyoteHandler");

            cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true, AllowAutoRedirect = true };
            hc = new HttpClient(handler);
            hc.DefaultRequestHeaders.Add("User-Agent", "Eleia/0.3");
        }

        public async Task Login(string username, string password)
        {
            await GetCsrfToken();

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", csrfToken),
                new KeyValuePair<string, string>("name", username),
                new KeyValuePair<string, string>("password", password),
            });

            var loginResult = await hc.PostAsync(Endpoints.LoginPage, data);
            var loginResultContent = await loginResult.Content.ReadAsStringAsync();

            if (!loginResult.IsSuccessStatusCode)
            {
                _logger.LogError("Login was not successful. Status code: {0}", loginResult.StatusCode);
            }

            await Task.Delay(Delay);
        }

        public async Task PostComment(Post post, string comment)
        {
            await GetCsrfToken();

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("post_id", post.id.ToString()),
                new KeyValuePair<string, string>("text", comment)
            });

            hc.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            hc.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrfToken);
            var commentResult = await hc.PostAsync(Endpoints.CommentPage, data);

            if (!commentResult.IsSuccessStatusCode)
            {
                _logger.LogError("Posting comment was not successful. Status code: {0}", commentResult.StatusCode);
            }

            await Task.Delay(Delay);
        }

        private async Task GetCsrfToken()
        {
            RemoveXHeaders();
            var mainPage = await hc.GetAsync(Endpoints.MainPage).Result.Content.ReadAsStringAsync();

            var hap = new HtmlDocument();
            hap.LoadHtml(mainPage);
            csrfToken = hap.DocumentNode.SelectSingleNode("/html/head/meta[4]").Attributes[1].Value;
            await Task.Delay(Delay);
        }

        private void RemoveXHeaders()
        {
            hc.DefaultRequestHeaders.Remove("X-Requested-With");
            hc.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        }

        public async void Logout()
        {
            await GetCsrfToken();

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", csrfToken)
            });

            var loginResult = await hc.PostAsync("http://dev.4programmers.info/Logout", data);
            var loginResultContent = await loginResult.Content.ReadAsStringAsync();
        }

        public async Task<IEnumerable<Post>> GetPosts()
        {
            RemoveXHeaders();
            var json = await hc.GetStringAsync(CoyoteApi.Endpoints.PostsApi);
            var result = JsonConvert.DeserializeObject<CoyoteApi.PostsApiResult>(json);

            return result.data;
        }
    }
}