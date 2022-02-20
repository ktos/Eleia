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

using CommandLine;
using Eleia.CoyoteApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Eleia
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class Program
    {
        private const string AnalyzedDatabasePath = "analyzed.bin";
        private static int timeBetweenUpdates;

        private static Bot bot;
        private static ILogger logger;

        private static bool isRunOnce = false;
        private static bool isConfigured = false;
        private static bool isRunSet = false;
        private static int[] runSet;

        public class Options
        {
            [Option('u', "username", Default = null, HelpText = "Username to log in with", Required = false)]
            public string UserName { get; set; }

            [Option('p', "password", Default = null, HelpText = "Password to log in with", Required = false)]
            public string Password { get; set; }

            [Option('t', "timeBetweenUpdates", Default = null, HelpText = "Time (in minutes) to wait before getting another set of posts", Required = false)]
            public int? TimeBetweenUpdates { get; set; }

            [Option('n', "nagMessage", Default = null, HelpText = "Message to be posted in comment to post", Required = false)]
            public string NagMessage { get; set; }

            [Option('d', "useDebug4p", Default = null, HelpText = "Should be used dev.4programmers.info?", Required = false)]
            public bool? UseDebug4p { get; set; }

            [Option('c', "postComments", Default = null, HelpText = "Should comments be posted to website?", Required = false)]
            public bool? PostComments { get; set; }

            [Option('r', "runOnce", Default = false, HelpText = "Should the application run only once, or loop?", Required = false)]
            public bool RunOnce { get; set; }

            [Option('s', "runOnSet", HelpText = "Enables single run on a defined set of post ids, separated by comma.", Required = false)]
            public string RunOnSet { get; set; }

            [Option("blacklist", HelpText = "Disallows defined set of forum ids, separated by comma.", Required = false)]
            public string Blacklist { get; set; }

            [Option("ignoreAlreadyAnalyzed", HelpText = "Ignores already analyzed posts database.", Required = false, Default = false)]
            public bool IgnoreAlreadyAnalyzed { get; set; }

            [Option("displayPrediction", HelpText = "Always displays prediction value in logs.", Required = false, Default = false)]
            public bool DisplayPrediction { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static void Main(string[] args)
        {
            Console.CancelKeyPress += RequestApplicationClose;
            var opts = Parser.Default.ParseArguments<Options>(args).WithParsed(Configure);

            if (!isConfigured)
                return;

            bot.LoadAlreadyAnalyzed();

            if (isRunSet)
            {
                try
                {
                    bot.AnalyzePostsByIdsAsync(runSet).Wait();
                }
                catch (AggregateException e)
                {
                    logger.LogError($"HTTP Exception: {e.Message}");
                }
                return;
            }



            if (isRunOnce)
            {
                try
                {
                    bot.AnalyzeNewPostsAsync().Wait();
                    logger.LogDebug("Single run completed.");
                }
                catch (AggregateException e)
                {
                    logger.LogError($"HTTP Exception: {e.Message}");
                }
            }
            else
            {
                while (true)
                {
                    try
                    {
                        bot.AnalyzeNewPostsAsync().Wait();
                    }
                    catch (AggregateException e)
                    {
                        logger.LogError($"HTTP Exception: {e.Message}");
                    }
                    finally
                    {
                        logger.LogDebug("Going to sleep for {0} minutes", timeBetweenUpdates);
                        Task.Delay(TimeSpan.FromMinutes(timeBetweenUpdates)).Wait();
                    }
                }
            }

            bot.SaveAlreadyAnalyzed();
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static void RequestApplicationClose(object sender, ConsoleCancelEventArgs e)
        {
            logger?.LogDebug("Requested application close by {0}", e.SpecialKey);
            bot?.SaveAlreadyAnalyzed();
            Environment.Exit(0);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1090:Call 'ConfigureAwait(false)'.", Justification = "Not a library")]
        private static void Configure(Options opts)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("ELEIA_")
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder
                    .AddConfiguration(config.GetSection("Logging"))
                    .AddConsole())
                .AddSingleton(config)
                .AddSingleton<CoyoteHandler>()
                .AddSingleton<PostAnalyzer>()
                .AddSingleton<Bot>()
                .AddSingleton<Blacklist>()
                .BuildServiceProvider();

            bot = serviceProvider.GetService<Bot>();

            bot.AnalyzedDatabasePath = AnalyzedDatabasePath;

            logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Eleia");

            var username = config.GetValue("username", opts.UserName);
            var password = config.GetValue("password", opts.Password);
            timeBetweenUpdates = config.GetValue("timeBetweenUpdates", opts.TimeBetweenUpdates ?? 60);

            bot.NagMessage = config.GetValue("nagMessage", "Hej! Twój post prawdopodobnie zawiera niesformatowany kod. Użyj znaczników ``` aby oznaczyć, co jest kodem, będzie łatwiej czytać. (jestem botem, ta akcja została wykonana automatycznie, prawdopodobieństwo {0})");

            Endpoints.IsDebug = config.GetValue("useDebug4p", opts.UseDebug4p ?? true);
            bot.PostComments = config.GetValue("postComments", opts.PostComments ?? false);

            logger.LogInformation("Eleia is running...");

            if (bot.PostComments && (username == null || password == null))
            {
                logger.LogError("Username or password is not provided, but posting comments is set. Exiting.");
                Thread.Sleep(100);
                Environment.Exit(1);
            }

            logger.LogDebug("Will use username: {0}, will post comments: {1}, will use real 4programmers.net: {2}", username, bot.PostComments, !Endpoints.IsDebug);

            if (bot.PostComments)
                bot.LoginAsync(username, password).Wait();

            isRunOnce = opts.RunOnce || timeBetweenUpdates == 0;

            if (!string.IsNullOrEmpty(opts.RunOnSet))
            {
                runSet = opts.RunOnSet.Split(',').Select(x => int.Parse(x)).ToArray();
                if (runSet != null)
                    isRunSet = true;
            }

            var blacklistDefinition = config.GetValue("blacklist", opts.Blacklist ?? string.Empty);
            if (!string.IsNullOrEmpty(blacklistDefinition))
                bot.BlacklistDefinition = blacklistDefinition;

            bot.IgnoreAlreadyAnalyzed = opts.IgnoreAlreadyAnalyzed;
            bot.DisplayPrediction = opts.DisplayPrediction;

            isConfigured = true;
        }
    }
}