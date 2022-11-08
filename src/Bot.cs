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
using MessagePack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        /// <summary>
        /// Database with already analyzed post ids
        /// A set of post ids which have been already analyzed so they won't
        /// be analyzed again
        /// </summary>
        public HashSet<int> AlreadyAnalyzedDatabase { get; set; }

        /// <summary>
        /// Gets or sets if the database of already analyzed posts should be used
        /// </summary>
        public bool IgnoreAlreadyAnalyzed { get; set; }

        public bool DisplayPrediction { get; set; }

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
            set
            {
                blacklistDefinition = value;
                blacklist.UpdateDefinition(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of Eleia.Bot
        /// </summary>
        /// <param name="handler">CoyoteHandler to work with Coyote</param>
        /// <param name="analyzer">PostAnalyzer for analyzing posts</param>
        /// <param name="list">Blacklist for blacklisting forums, posts or so on</param>
        /// <param name="loggerFactory">LoggerFactory for login purposes</param>
        public Bot(
            CoyoteHandler handler,
            PostAnalyzer analyzer,
            Blacklist list,
            ILoggerFactory loggerFactory
        )
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
                AlreadyAnalyzedDatabase = new HashSet<int>();
                return;
            }

            if (!File.Exists(AnalyzedDatabasePath))
            {
                AlreadyAnalyzedDatabase = new HashSet<int>();
            }
            else
            {
                logger.LogDebug("Loading already analyzed posts database.");
                
                if (!File.Exists(AnalyzedDatabasePath))
                {
                    AlreadyAnalyzedDatabase = new HashSet<int>();
                    return;
                }
                else
                {
                    var bytes = File.ReadAllBytes(AnalyzedDatabasePath);
                    AlreadyAnalyzedDatabase = MessagePackSerializer.Deserialize<HashSet<int>>(bytes);
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
            
            logger.LogDebug("Saving already analyzed ids database.");

            byte[] bytes = MessagePackSerializer.Serialize(AlreadyAnalyzedDatabase);
            File.WriteAllBytes(AnalyzedDatabasePath, bytes);            
        }

        /// <summary>
        /// Analyzes a set of posts based on their ids, one by one
        /// </summary>
        /// <param name="postIds">A list of post ids to analyze</param>
        public async Task AnalyzePostsByIdsAsync(int[] postIds)
        {
            foreach (
                var item in postIds.Select(
                    async x => await coyoteHandler.GetSinglePost(x).ConfigureAwait(false)
                )
            )
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
            if (AlreadyAnalyzedDatabase.Contains(post.id))
            {
                logger.LogDebug("Ignoring post {0} because already analyzed", post.id);
                return;
            }

            AlreadyAnalyzedDatabase.Add(post.id);
            SaveAlreadyAnalyzed();

            if (IgnorePost(post))
            {
                logger.LogDebug("Ignoring post {0} because of blacklist", post.id);
                return;
            }

            logger.LogInformation("Analyzing post {0}", post.id);
            var analysisResult = postAnalyzer.AnalyzeText(post);

            if (analysisResult.Prediction)
            {
                logger.LogWarning(
                    "Found problems in post {0} {1} -- {2}",
                    post.id,
                    post.url,
                    analysisResult.Item
                );

                if (PostComments)
                {
                    logger.LogDebug("Posting comment");
                    await coyoteHandler
                        .PostComment(post, string.Format(NagMessage, analysisResult.Probability))
                        .ConfigureAwait(false);
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
            return blacklist.IsDisallowed(post);
        }

        /// <summary>
        /// Get new posts from the last first page of posts set and analyze them
        /// </summary>
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
