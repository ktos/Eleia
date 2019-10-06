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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Eleia
{
    internal static class Program
    {
        private const int TimeBetweenUpdates = 1;
        private const int DelayBetweenTopicScrape = 2000;

        private static HashSet<string> analyzedTopics;
        private static HashSet<string> ignoredTopics;

        private static HttpClient hc;
        private static ILogger logger;

        private static void Main(string[] args)
        {
            hc = new HttpClient();
            hc.DefaultRequestHeaders.Add("User-Agent", "Eleia/0.1");
            analyzedTopics = new HashSet<string>();
            ignoredTopics = new HashSet<string>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            logger = loggerFactory.CreateLogger("Eleia");

            logger.LogInformation("Eleia is running...");

            ignoredTopics.Add("https://4programmers.net/Forum/C_i_.NET/121282-Kursy_visual_c");
            ignoredTopics.Add("https://4programmers.net/Forum/C_i_.NET/160647-Materialy_dostepne_w_sieci");
            ignoredTopics.Add("https://4programmers.net/Forum/C_i_.NET/192417-darmowe_pluginy_do_visual_studio_");
            ignoredTopics.Add("https://4programmers.net/Forum/C_i_.NET/196733-o_naduzywaniu_c++cli");

            while (true)
            {
                ScrapAndAnalyzeNew();
                Thread.Sleep(TimeSpan.FromMinutes(TimeBetweenUpdates));
            }
        }

        private static async void ScrapAndAnalyzeNew()
        {
            logger.LogInformation("Getting new topics...");
            var newTopics = await GetNewTopics("https://4programmers.net/Forum/C_i_.NET?perPage=10");
            foreach (var topic in newTopics)
            {
                AnalyzeTopic(topic);

                await Task.Delay(DelayBetweenTopicScrape);
            }
        }

        private static async void AnalyzeTopic(string url)
        {
            if (analyzedTopics.Contains(url))
                return;

            analyzedTopics.Add(url);

            var html = await hc.GetStringAsync(url);

            logger.LogInformation($"Analyzing topic {url}");
            TopicAnalyzer.Analyze(html);
        }

        private static async Task<List<string>> GetNewTopics(string url)
        {
            var output = new List<string>();

            var html = await hc.GetStringAsync(url);

            foreach (var topicUrl in SubforumAnalyzer.GetTopics(html))
            {
                if (!analyzedTopics.Contains(topicUrl) && !ignoredTopics.Contains(topicUrl))
                {
                    output.Add(topicUrl);
                }
            }

            return output;
        }
    }
}