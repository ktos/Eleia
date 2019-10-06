using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
            analyzedTopics = new HashSet<string>();
            ignoredTopics = new HashSet<string>();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
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