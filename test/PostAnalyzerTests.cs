using Eleia;
using Eleia.CoyoteApi;
using System;
using Xunit;

namespace Eleia.Test
{
    public class PostAnalyzerTests
    {
        [Fact]
        public void Analyze_PostWithQuotes_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer();
            Post post = new Post { text = "<blockquote>\n<h5><a href=\"https://4programmers.net/Forum/1625684\">cmd napisał(a)</a>:</h5>\n<p>U mnie w firmie było do przejścia jedno zadanie na tablicy, był to fizzbuzz</p>\n</blockquote>\n<p>ale to tylko dla juniorów tak? czy mid deweloperzy tez u was fizzfuzz nie przechodzili?</p>" };

            var result = postAnalyzer.Analyze(post);

            Assert.Empty(result);
        }

        [Fact]
        public void Analyze_SimplePost_NoProblems()
        {
            var postAnalyzer = new PostAnalyzer();
            Post post = new Post { text = "<p>A to Kotlin już nie na topie?</p>" };

            var result = postAnalyzer.Analyze(post);

            Assert.Empty(result);
        }
    }
}