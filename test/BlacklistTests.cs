using Eleia;
using Eleia.CoyoteApi;
using System;
using Xunit;

namespace Eleia.Test
{
    public class BlacklistTests
    {
        [Fact]
        public void IsDisallowed_OneForumIdMatched_Blocked()
        {
            // Arrange
            var blacklist = new Blacklist("1");
            var post = new Post { forum_id = 1 };

            // Act
            var result = blacklist.IsDisallowed(post);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsDisallowed_OneForumIdNotMatched_Allowed()
        {
            var blacklist = new Blacklist("1");
            var post = new Post { forum_id = 2 };

            var result = blacklist.IsDisallowed(post);

            Assert.False(result);
        }

        [Fact]
        public void IsDisallowed_ManyForumIdsNotMatched_AllAllowed()
        {
            var blacklist = new Blacklist("1,2,3");
            var posts = new Post[] {
                new Post { forum_id = 4 },
                new Post { forum_id = 5 },
                new Post { forum_id = 6 },
                new Post { forum_id = 7 },
            };

            Assert.False(blacklist.IsDisallowed(posts[0]));
            Assert.False(blacklist.IsDisallowed(posts[1]));
            Assert.False(blacklist.IsDisallowed(posts[2]));
            Assert.False(blacklist.IsDisallowed(posts[3]));
        }

        [Fact]
        public void IsDisallowed_ManyForumIdsMatched_AllBlocked()
        {
            var blacklist = new Blacklist("1,2,3,4");
            var posts = new Post[] {
                new Post { forum_id = 1 },
                new Post { forum_id = 2 },
                new Post { forum_id = 3 },
                new Post { forum_id = 4 },
            };

            Assert.True(blacklist.IsDisallowed(posts[0]));
            Assert.True(blacklist.IsDisallowed(posts[1]));
            Assert.True(blacklist.IsDisallowed(posts[2]));
            Assert.True(blacklist.IsDisallowed(posts[3]));
        }
    }
}