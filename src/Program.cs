#region License

/*
 * Eleia
 *
 * Copyright (C) Marcin Badurowicz <m at badurowicz dot net> 2019
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#endregion License

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Eleia
{
    internal static class Program
    {
        private const int TimeBetweenUpdates = 60;
        private const int DelayBetweenTopicScrape = 2000;

        private static HashSet<int> analyzed;

        private static HttpClient hc;
        private static ILogger logger;

        private static PostAnalyzer pa;

        private static void Main(string[] args)
        {
            hc = new HttpClient();
            hc.DefaultRequestHeaders.Add("User-Agent", "Eleia/0.2");
            analyzed = new HashSet<int>();

            pa = new PostAnalyzer();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            logger = loggerFactory.CreateLogger("Eleia");

            logger.LogInformation("Eleia is running...");

            while (true)
            {
                AnalyzeNewPosts();
                Thread.Sleep(TimeSpan.FromMinutes(TimeBetweenUpdates));
            }
        }

        private static async void AnalyzeNewPosts()
        {
            logger.LogInformation("Getting posts...");
            var posts = await GetPosts();

            foreach (var post in posts)
            {
                logger.LogInformation("Analyzing post {0}", post.id);
                AnalyzePost(post);
            }
        }

        private static void AnalyzePost(CoyoteApi.Post post)
        {
            if (analyzed.Contains(post.id))
                return;

            analyzed.Add(post.id);
            var problems = pa.Analyze(post);

            if (problems.Count > 0)
            {
                Console.WriteLine("Problems found in post id: {0}", post.id);
                foreach (var item in problems)
                {
                    Console.WriteLine(item.ToString());
                }
            }
        }

        private static async Task<IEnumerable<CoyoteApi.Post>> GetPosts()
        {
            var json = await hc.GetStringAsync(CoyoteApi.Endpoints.PostsApi);
            var result = JsonConvert.DeserializeObject<CoyoteApi.PostsApiResult>(json);

            return result.data;
        }
    }
}