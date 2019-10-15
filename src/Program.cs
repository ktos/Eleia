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

using Eleia.CoyoteApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Eleia
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static int timeBetweenUpdates = 60;

        private static HashSet<int> analyzed;

        private static ILogger logger;

        private static PostAnalyzer pa;
        private static CoyoteHandler ch;

        private static void Main(string[] args)
        {
            analyzed = new HashSet<int>();

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables("ELEIA_")
                .AddCommandLine(args)
                .Build();

            var username = config.GetValue<string>("username");
            var password = config.GetValue<string>("password");
            var threshold = config.GetValue<float>("threshold");
            timeBetweenUpdates = config.GetValue<int>("timeBetweenUpdates");

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => { builder.AddConsole(); })
                .AddTransient<CoyoteHandler>()
                .AddTransient<PostAnalyzer>()
                .BuildServiceProvider();

            ch = serviceProvider.GetService<CoyoteHandler>();
            pa = serviceProvider.GetService<PostAnalyzer>();

            //ch.Login(username, password).Wait();

            logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Eleia");
            logger.LogInformation("Eleia is running...");

            while (true)
            {
                AnalyzeNewPosts();
                Thread.Sleep(TimeSpan.FromMinutes(timeBetweenUpdates));
            }
        }

        private static async void AnalyzeNewPosts()
        {
            logger.LogInformation("Getting posts...");
            var posts = await ch.GetPosts();

            foreach (var post in posts)
            {
                logger.LogInformation("Analyzing post {0}", post.id);
                await AnalyzePost(post);
            }
        }

        private static async Task AnalyzePost(CoyoteApi.Post post)
        {
            if (analyzed.Contains(post.id))
                return;

            analyzed.Add(post.id);
            var problems = pa.Analyze(post);

            if (problems.Count > 0)
            {
                Console.WriteLine("Problems found!");
                foreach (var item in problems)
                {
                    Console.WriteLine(item.ToString());
                    //await ch.PostComment(post, $"Hej! Twój post prawdopodobnie zawiera niesformatowany kod. Użyj znaczników ``` aby oznaczyć, co jest kodem, będzie łatwiej czytać. (jestem botem, ta akcja została wykonana automatycznie, prawdopodobieństwo {item.Probability})");
                }
                Console.WriteLine(post.url);
                Console.WriteLine(post.text.Length < 50 ? post.text : post.text.Substring(0, 50));
            }
        }
    }
}