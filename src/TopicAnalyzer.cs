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

using Eleia.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Eleia
{
    internal static class TopicAnalyzer
    {
        public static void Analyze(string html)
        {
            var hap = new HtmlAgilityPack.HtmlDocument();
            hap.LoadHtml(html);
            var allPosts = hap.DocumentNode.SelectNodes("//div[@data-post-id]");

            var posts = new Dictionary<string, List<string>>();

            foreach (var item in allPosts)
            {
                var postId = item.Attributes["data-post-id"].Value;
                var postContent = item.InnerHtml.Replace("\r", string.Empty);
                postContent = RemoveHtmlContent(postContent);

                var postParagraphs = CleanParagraph(postContent.Split("</p>").ToList());
                posts.Add(postId, postParagraphs);
            }

            foreach (var post in posts)
            {
                foreach (var item in post.Value)
                {
                    var input = new ModelInput();
                    input.Content = item;

                    ModelOutput result = ConsumeModel.Predict(input);

                    if (result.Prediction == "code" && result.Score[1] > 0.9)
                    {
                        Console.WriteLine($"Potentially not formatted code found in {post.Key} (prob: {result.Score[1]}): ");
                        Console.WriteLine(item);
                        break;
                    }
                }
            }
        }

        private static string RemoveHtmlContent(string posttext)
        {
            // removing every code properly put in the <code> or <pre><code class=""> tags
            posttext = Regex.Replace(posttext, "<pre><code(.|\n)*?</pre>", "", RegexOptions.Multiline);
            posttext = Regex.Replace(posttext, "<code(.|\n)*?</code>", "", RegexOptions.Multiline);

            // removing every link to attachment download
            posttext = Regex.Replace(posttext, "<i class=\"fa fa-download(.|\n)*?</li>", "", RegexOptions.Multiline);
            return posttext;
        }

        private static List<string> CleanParagraph(List<string> paras)
        {
            var output = new List<string>();

            foreach (var item in paras)
            {
                // strip tags
                var cleaned = Regex.Replace(item, "<.*?>", string.Empty);

                // strip multi spaces, new lines and everything what is empty
                cleaned = cleaned.Replace("\n", "");
                cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

                if (!string.IsNullOrWhiteSpace(cleaned))
                    output.Add(cleaned);
            }

            return output;
        }
    }
}