using Eleia;
using System;
using Xunit;

namespace Eleia.Test
{
    public class HtmlCleanerTests
    {
        [Fact]
        public void RemoveProperCode_PostWithProperTags_TagsRemoved()
        {
            string postText = "<code>test</code><pre><code>test2</pre></code><pre><code class=\"csharp\">test3</code></pre>";

            var result = HtmlCleaner.RemoveProperCode(postText);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void RemoveDownloadLinks_PostWithDownloadLinks_LinksRemoved()
        {
            string postText = "<li><i class=\"fa fa-download\">test</i>test2</li>test3";

            var result = HtmlCleaner.RemoveDownloadLinks(postText);

            Assert.Equal("<li>test3", result);
        }

        [Fact]
        public void StripTags_TextWithTags_TagsRemoved()
        {
            string item = "<p>some</p><b>text</b><strong>with</strong><a href=\"#\">tags</a><br /><br/>";

            var result = HtmlCleaner.StripTags(item);

            Assert.Equal("sometextwithtags", result);
        }

        [Fact]
        public void StripTags_TextWithTagsNotClosed_TagsRemoved()
        {
            string item = "<p></p></a></code></i><li><ul><ol></li><br /><br/>";

            var result = HtmlCleaner.StripTags(item);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void StripTags_TextWithoutTags_TagsNotRemoved()
        {
            string item = "some text without tags";

            var result = HtmlCleaner.StripTags(item);

            Assert.Equal(item, result);
        }

        [Fact]
        public void StripWhitespace_TextWithWhitespace_WhitespaceRemoved()
        {
            string item = "some  text   with   \nwhitespace   ";

            var result = HtmlCleaner.StripWhitespace(item);

            Assert.Equal("some text with whitespace", result);
        }

        [Fact]
        public void StripWhitespace_TextWithoutWhitespace_TextIntact()
        {
            string item = "some text without whitespace";

            var result = HtmlCleaner.StripWhitespace(item);

            Assert.Equal(item, result);
        }
    }
}