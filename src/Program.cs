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
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Eleia
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static int timeBetweenUpdates;

        private static HashSet<int> analyzed;

        private static ILogger logger;

        private static PostAnalyzer pa;
        private static CoyoteHandler ch;
        private static bool postComments;
        private static string nagMessage;

        private static void Main(string[] args)
        {
            analyzed = new HashSet<int>();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("ELEIA_")
                .AddCommandLine(args)
                .Build();

            var username = config.GetValue<string>("username");
            var password = config.GetValue<string>("password");
            timeBetweenUpdates = config.GetValue("timeBetweenUpdates", 60);

            nagMessage = config.GetValue("nagMessage", "Hej! Twój post prawdopodobnie zawiera niesformatowany kod. Użyj znaczników ``` aby oznaczyć, co jest kodem, będzie łatwiej czytać. (jestem botem, ta akcja została wykonana automatycznie, prawdopodobieństwo {0})");

            Endpoints.IsDebug = config.GetValue("useDebug4p", true);
            postComments = config.GetValue("postComments", false);

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder
                    .AddConfiguration(config.GetSection("Logging"))
                    .AddConsole())
                .AddSingleton(config)
                .AddTransient<CoyoteHandler>()
                .AddTransient<PostAnalyzer>()
                .BuildServiceProvider();

            ch = serviceProvider.GetService<CoyoteHandler>();
            pa = serviceProvider.GetService<PostAnalyzer>();

            logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Eleia");
            logger.LogInformation("Eleia is running...");

            if (postComments && (username == null || password == null))
            {
                logger.LogError("Username or password is not provided, but posting comments is set. Exiting.");
                Thread.Sleep(100);
                Environment.Exit(1);
            }

            logger.LogDebug("Will use username: {0}, will post comments: {1}, will use real 4programmers.net: {2}", username, postComments, !Endpoints.IsDebug);

            if (postComments)
                ch.Login(username, password).Wait();

            if (timeBetweenUpdates <= 0)
            {
                AnalyzeNewPosts().Wait();
                logger.LogDebug("Single run completed");
            }
            else if (timeBetweenUpdates > 0)
            {
                while (true)
                {
                    AnalyzeNewPosts().Wait();
                    logger.LogDebug("Going to sleep for {0} minutes", timeBetweenUpdates);
                    Thread.Sleep(TimeSpan.FromMinutes(timeBetweenUpdates));
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1090:Call 'ConfigureAwait(false)'.", Justification = "Not a library")]
        private static async Task AnalyzeNewPosts()
        {
            logger.LogDebug("Getting posts...");
            var posts = await ch.GetPosts();

            foreach (var post in posts)
            {
                await AnalyzePost(post);
            }
            logger.LogDebug("Analyzed (or ignored) everything");
        }

        private static bool IgnorePost(Post post)
        {
            // ignore everything not in C# subforum
            return !Endpoints.IsDebug && post.forum_id != 24;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1090:Call 'ConfigureAwait(false)'.", Justification = "Not a library")]
        private static async Task AnalyzePost(Post post)
        {
            if (analyzed.Contains(post.id))
                return;

            analyzed.Add(post.id);
            if (IgnorePost(post))
                return;

            logger.LogDebug("Analyzing post {0}", post.id);
            var problems = pa.Analyze(post);

            if (problems.Count > 0)
            {
                logger.LogInformation("Found problems in post {0}\n{1}\n{2}", post.id, post.url, post.text.Length < 50 ? post.text : post.text.Substring(0, 50));
                foreach (var item in problems)
                {
                    logger.LogInformation(item.ToString());

                    if (postComments)
                    {
                        logger.LogDebug("Posting comment");
                        await ch.PostComment(post, string.Format(nagMessage, item.Probability));
                    }
                }
            }
        }
    }
}