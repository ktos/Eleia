using Eleia;
using Eleia.CoyoteApi;
using Eleia.Test.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Eleia.Test
{
    public class BotTests
    {
        private FakeLoggerFactory fakeLoggerFactory;

        private Bot CreateBot()
        {
            fakeLoggerFactory = new FakeLoggerFactory();
            return new Bot(new CoyoteHandler(fakeLoggerFactory), new PostAnalyzer(null), new Blacklist(), fakeLoggerFactory);
        }

        private List<string> GetFakeLogs()
        {
            return (fakeLoggerFactory.CreateLogger("") as FakeLogger).Logs;
        }

        [Fact]
        public void LoadAlreadyAnalyzed_WhenIgnoreAlreadyAnalyzed_ShouldBeEmpty()
        {
            var bot = CreateBot();
            bot.IgnoreAlreadyAnalyzed = true;

            bot.LoadAlreadyAnalyzed();

            Assert.Empty(bot.AlreadyAnalyzedDatabase);
        }

        [Fact]
        public void SaveAlreadyAnalyzed_WhenIgnoreAlreadyAnalyzed_ShouldDoNothing()
        {
            var bot = CreateBot();
            bot.IgnoreAlreadyAnalyzed = true;

            bot.SaveAlreadyAnalyzed();

            var logs = GetFakeLogs();

            Assert.DoesNotContain(logs, x => x == "Saving already analyzed ids database.");
        }

        [Fact]
        public void BlacklistUpdateDefinition_ShouldUpdateDefinition()
        {
            var bot = CreateBot();
            bot.IgnoreAlreadyAnalyzed = true;

            var old = bot.BlacklistDefinition;
            bot.BlacklistDefinition = "1,2,3,4";

            Assert.NotEqual(bot.BlacklistDefinition, old);
            Assert.Equal("1,2,3,4", bot.BlacklistDefinition);
        }

        [Fact]
        public async void AnalyzePostAsync_Post_ShouldUpdateAlreadyAnalyzed()
        {
            var bot = CreateBot();
            bot.IgnoreAlreadyAnalyzed = true;
            bot.LoadAlreadyAnalyzed();

            await bot.AnalyzePostAsync(new Post() { id = 1, text = "" });

            Assert.Single(bot.AlreadyAnalyzedDatabase);
            Assert.Equal(1, bot.AlreadyAnalyzedDatabase.First());
        }

        [Fact]
        public async void AnalyzePostAsync_SamePostIdTestedForSecondTime_ShouldBeIgnored()
        {
            var bot = CreateBot();
            bot.IgnoreAlreadyAnalyzed = true;
            bot.LoadAlreadyAnalyzed();

            await bot.AnalyzePostAsync(new Post() { id = 1, text = "one text" });
            await bot.AnalyzePostAsync(new Post() { id = 1, text = "different text" });

            var logs = GetFakeLogs();

            Assert.Contains(logs, x => x == "[Debug] Ignoring post 1 because already analyzed");
        }

        [Fact]
        public async void AnalyzePostAsync_PostFromBlacklist_ShouldBeIgnored()
        {
            var bot = CreateBot();
            bot.IgnoreAlreadyAnalyzed = true;
            bot.LoadAlreadyAnalyzed();
            bot.BlacklistDefinition = "1";

            await bot.AnalyzePostAsync(new Post() { id = 1, text = "", forum_id = 1 });
            var logs = GetFakeLogs();

            Assert.Equal("[Debug] Ignoring post 1 because of blacklist", logs.Last());
        }

        [Fact]
        public void Endpoints_WhenNotDebug_ShouldBeBasedOn4pnet()
        {
            Endpoints.IsDebug = false;

            Assert.Contains("4programmers.net", Endpoints.CommentPage);
            Assert.Contains("4programmers.net", Endpoints.LoginPage);
            Assert.Contains("4programmers.net", Endpoints.MainPage);
            Assert.Contains("4programmers.net", Endpoints.LogoutPage);
            Assert.Contains("4programmers.net", Endpoints.PostsApi);
        }

        [Fact]
        public void Endpoints_WhenDebug_ShouldBeBasedOn4pinfo()
        {
            Endpoints.IsDebug = true;

            Assert.Contains("4programmers.dev", Endpoints.CommentPage);
            Assert.Contains("4programmers.dev", Endpoints.LoginPage);
            Assert.Contains("4programmers.dev", Endpoints.MainPage);
            Assert.Contains("4programmers.dev", Endpoints.LogoutPage);
            Assert.Contains("4programmers.dev", Endpoints.PostsApi);
        }
    }
}