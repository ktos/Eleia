﻿using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Eleia.CoyoteApi
{
    /// <summary>
    /// Class for communication with Coyote
    /// </summary>
    public class CoyoteHandler
    {
        private readonly HttpClient hc;
        private readonly ILogger _logger;
        private readonly CookieContainer cookieContainer;
        private string csrfToken;

        /// <summary>
        /// Delay between operations performed
        /// </summary>
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Creates a new instance of CoyoteHandler with a logging factory to provide ILogger
        /// </summary>
        /// <param name="logger">Logging factory for debug purposes</param>
        public CoyoteHandler(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger("CoyoteHandler");

            cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true, AllowAutoRedirect = true };
            hc = new HttpClient(handler);

            var userAgent = BuildUserAgent();
            hc.DefaultRequestHeaders.Add("User-Agent", userAgent);
            _logger.LogDebug($"User-Agent to be used: {userAgent}");
        }

        private string BuildUserAgent()
        {
            var semVer = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var platform = Environment.OSVersion.Platform.ToString();

            return $"Eleia/{semVer} ({platform})";
        }

        /// <summary>
        /// Performs a log in to a Coyote system
        /// </summary>
        /// <param name="username">Username to be authenticated with</param>
        /// <param name="password">Password to be authenticated with</param>
        public async Task Login(string username, string password)
        {
            await GetCsrfToken().ConfigureAwait(false);

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", csrfToken),
                new KeyValuePair<string, string>("name", username),
                new KeyValuePair<string, string>("password", password),
            });

            _logger.LogDebug("Performing login");
            var loginResult = await hc.PostAsync(Endpoints.LoginPage, data).ConfigureAwait(false);

            if (!loginResult.IsSuccessStatusCode)
            {
                _logger.LogError("Login was not successful. Status code: {0}", loginResult.StatusCode);
            }

            await Task.Delay(Delay).ConfigureAwait(false);
        }

        /// <summary>
        /// Posts a comment to a specified post in the system
        /// </summary>
        /// <param name="post">Post we are commenting</param>
        /// <param name="comment">Text of the comment</param>
        public async Task PostComment(Post post, string comment)
        {
            await GetCsrfToken().ConfigureAwait(false);

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("post_id", post.id.ToString()),
                new KeyValuePair<string, string>("text", comment)
            });

            _logger.LogDebug("Posting comment");
            hc.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            hc.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrfToken);
            var commentResult = await hc.PostAsync(Endpoints.CommentPage, data).ConfigureAwait(false);

            if (!commentResult.IsSuccessStatusCode)
            {
                _logger.LogError("Posting comment was not successful. Status code: {0}", commentResult.StatusCode);
            }
            else
            {
                _logger.LogInformation("Posted comment to post {0}", post.id);
            }

            await Task.Delay(Delay).ConfigureAwait(false);
        }

        private async Task GetCsrfToken()
        {
            _logger.LogDebug("Getting CSRF token");
            RemoveXHeaders();
            var mainPage = await hc.GetAsync(Endpoints.MainPage).Result.Content.ReadAsStringAsync().ConfigureAwait(false);

            var hap = new HtmlDocument();
            hap.LoadHtml(mainPage);
            csrfToken = hap.DocumentNode.SelectSingleNode("/html/head/meta[4]").Attributes[1].Value;
            await Task.Delay(Delay).ConfigureAwait(false);
        }

        private void RemoveXHeaders()
        {
            hc.DefaultRequestHeaders.Remove("X-Requested-With");
            hc.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        }

        /// <summary>
        /// Logs out of Coyote
        /// </summary>
        public async void Logout()
        {
            await GetCsrfToken().ConfigureAwait(false);

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("_token", csrfToken)
            });

            _logger.LogDebug("Performing logout");
            await hc.PostAsync(Endpoints.LogoutPage, data).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets list of last posts from the Coyote API
        /// </summary>
        /// <returns>List of newest posts in the system</returns>
        public async Task<IEnumerable<Post>> GetPosts(int page = 1)
        {
            RemoveXHeaders();
            _logger.LogDebug("Getting new posts");
            var json = await hc.GetStringAsync($"{Endpoints.PostsApi}?page={page}").ConfigureAwait(false);

            var result = JsonSerializer.Deserialize<PostsApiResult>(json);

            return result.data;
        }
    }
}