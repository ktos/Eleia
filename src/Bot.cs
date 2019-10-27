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
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Eleia
{
    /// <summary>
    /// Bot for analyzing posts on 4programmers.net looking for problems
    /// </summary>
    public class Bot
    {
        private readonly CoyoteHandler coyoteHandler;
        private readonly PostAnalyzer postAnalyzer;
        private readonly Blacklist blacklist;
        private readonly ILogger logger;
        private HashSet<int> analyzed;

        /// <summary>
        /// Gets or sets if the database of already analyzed posts should be used
        /// </summary>
        public bool IgnoreAlreadyAnalyzed { get; set; }

        /// <summary>
        /// Gets or sets the path to the already analyzed posts database
        /// </summary>
        public string AnalyzedDatabasePath { get; set; }

        /// <summary>
        /// Gets or sets if comments to the post should be posted
        /// </summary>
        public bool PostComments { get; set; }

        /// <summary>
        /// Gets or sets the nag message which is published as a problematic post comment
        /// </summary>
        public string NagMessage { get; set; }

        private string blacklistDefinition;

        /// <summary>
        /// Gets or sets definition of blacklist - ids of forums from which posts are ignored
        /// </summary>
        public string BlacklistDefinition
        {
            get { return blacklistDefinition; }
            set { blacklistDefinition = value; blacklist.UpdateDefinition(value); }
        }

        /// <summary>
        /// Initializes a new instance of Eleia.Bot
        /// </summary>
        /// <param name="handler">CoyoteHandler to work with Coyote</param>
        /// <param name="analyzer">PostAnalyzer for analyzing posts</param>
        /// <param name="list">Blacklist for blacklisting forums, posts or so on</param>
        /// <param name="loggerFactory">LoggerFactory for login purposes</param>
        public Bot(CoyoteHandler handler, PostAnalyzer analyzer, Blacklist list, ILoggerFactory loggerFactory)
        {
            coyoteHandler = handler;
            postAnalyzer = analyzer;
            blacklist = list;
            logger = loggerFactory.CreateLogger("Bot");
        }

        /// <summary>
        /// Loads already analyzed posts database from file
        /// </summary>
        public void LoadAlreadyAnalyzed()
        {
            if (IgnoreAlreadyAnalyzed)
            {
                analyzed = new HashSet<int>();
                return;
            }

            if (!File.Exists(AnalyzedDatabasePath))
            {
                analyzed = new HashSet<int>();
            }
            else
            {
                logger.LogDebug("Loading already analyzed posts database.");
                using var fs = new FileStream(AnalyzedDatabasePath, FileMode.Open, FileAccess.Read);
                if (fs.Length == 0)
                {
                    analyzed = new HashSet<int>();
                    return;
                }
                else
                {
                    var formatter = new BinaryFormatter();
                    analyzed = formatter.Deserialize(fs) as HashSet<int>;
                }
            }
        }

        /// <summary>
        /// Saves already analyzed posts database
        /// </summary>
        public void SaveAlreadyAnalyzed()
        {
            if (IgnoreAlreadyAnalyzed)
                return;

            using var fs = new FileStream(AnalyzedDatabasePath, FileMode.Create, FileAccess.Write);
            logger.LogDebug("Saving already analyzed ids database.");
            var formatter = new BinaryFormatter();
            formatter.Serialize(fs, analyzed);
        }

        /// <summary>
        /// Analyzes a set of posts based on their ids, one by one
        /// </summary>
        /// <param name="postIds">A list of post ids to analyze</param>
        public async Task AnalyzePostsByIdsAsync(int[] postIds)
        {
            foreach (var item in postIds.Select(async x => await coyoteHandler.GetSinglePost(x).ConfigureAwait(false)))
            {
                await AnalyzePostAsync(item.Result).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Analyze a single post and post comment to it if needed
        /// </summary>
        /// <param name="post">Post to be analyzed</param>
        public async Task AnalyzePostAsync(Post post)
        {
            if (analyzed.Contains(post.id))
            {
                logger.LogDebug("Ignoring post {0} because already analyzed", post.id);
                return;
            }

            analyzed.Add(post.id);
            SaveAlreadyAnalyzed();

            if (IgnorePost(post))
            {
                logger.LogDebug("Ignoring post {0} because of blacklist", post.id);
                return;
            }

            logger.LogInformation("Analyzing post {0}", post.id);
            var problems = postAnalyzer.Analyze(post);

            if (problems.Count > 0)
            {
                logger.LogInformation("Found problems in post {0}\n{1}\n{2}", post.id, post.url, post.text.Length < 50 ? post.text : post.text.Substring(0, 50));
                foreach (var item in problems)
                {
                    logger.LogDebug(item.ToString());

                    if (PostComments)
                    {
                        logger.LogInformation("Posting comment");
                        await coyoteHandler.PostComment(post, string.Format(NagMessage, item.Probability)).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Logs in to the Coyote system
        /// </summary>
        /// <param name="username">Username to log in with</param>
        /// <param name="password">Password to log in with</param>
        public async Task LoginAsync(string username, string password)
        {
            await coyoteHandler.Login(username, password).ConfigureAwait(false);
        }

        private bool IgnorePost(Post post)
        {
            if (blacklist == null)
                return false;
            else
                return blacklist.IsDisallowed(post);
        }

        /// <summary>
        /// Get new posts from the last first page of posts set and analyze them
        /// </summary>
        /// <returns></returns>
        public async Task AnalyzeNewPostsAsync()
        {
            logger.LogDebug("Getting posts...");
            var posts = await coyoteHandler.GetPosts().ConfigureAwait(false);

            foreach (var post in posts)
            {
                await AnalyzePostAsync(post).ConfigureAwait(false);
            }
            logger.LogDebug("Analyzed (or ignored) everything");
        }
    }
}