using Eleia;
using System;
using Xunit;

namespace Eleia.Test
{
    public class TextCleanerTests
    {
        [Fact]
        public void RemoveProperCode_PostWithProperTags_TagsRemoved()
        {
            string postText = "```test``````test2``````csharptest3```";

            var result = TextCleaner.RemoveProperCode(postText);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void RemoveMarkdownContent_PostWithQuotes_QuotesRemoved()
        {
            string postText = "> test\n>test\n>testtest\nnie cytat";

            var result = TextCleaner.RemoveMarkdownContent(postText);

            Assert.Equal("nie cytat", result);
        }

        [Fact]
        public void RemoveMarkdownContent_PostWithList_ListRemoved()
        {
            string postText = "* tak\n* nie, ale\n*tak i tak i tak";

            var result = TextCleaner.RemoveMarkdownContent(postText);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void RemoveMarkdownContent_PostWithLinks_LinksRemoved()
        {
            string postText = "bla bla http://example.com bla bla https://example.com bla https://example.com";

            var result = TextCleaner.RemoveMarkdownContent(postText);

            Assert.Equal("bla bla bla bla bla ", result);
        }

        [Fact]
        public void RemoveMarkdownContent_PostWithImage_ImageRemoved()
        {
            string postText = "![screenshot-20220223162536.png](https://4programmers.net/uploads/116718/hT5MNDVhehI4cnrAvgSHA3U6bS2SUAeeTN7Js6tr.png)";

            var result = TextCleaner.RemoveMarkdownContent(postText);

            Assert.Equal(string.Empty, result);
        }
    }
}