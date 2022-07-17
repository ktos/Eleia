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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Eleia
{
    /// <summary>
    /// Removes elements from cleartext of a post
    /// </summary>
    public static class TextCleaner
    {
        /// <summary>
        /// Removes properly formatted code from a post (with ``` tag)
        /// </summary>
        /// <param name="postText">Post content to be cleaned</param>
        /// <returns>Post content with ` and ``` elements removed</returns>
        public static string RemoveProperCode(string postText)
        {
            postText = Regex.Replace(postText, "`(.|\n)*?`", "", RegexOptions.Multiline);
            postText = Regex.Replace(postText, "```(.|\n)*?```", "", RegexOptions.Multiline);
            return postText;
        }

        /// <summary>
        /// Removes quotations, lists and other HTML tags which content should be ignored
        /// </summary>
        /// <param name="postText">Post content to be cleaned (in HTML)</param>
        /// <returns>Post content with Blockquote elements removed</returns>
        public static string RemoveMarkdownContent(string postText)
        {
            // removing quotes
            postText = Regex.Replace(postText, ">(.*?)\n", "");

            // removing lists
            postText = Regex.Replace(postText, "\\*(.*?)\n", "");
            postText = Regex.Replace(postText, "\\*(.*?)$", "");

            // removing images
            postText = Regex.Replace(postText, "!\\[(.*?)\\]\\(.*?\\)", "", RegexOptions.Multiline);

            // removing unformatted links
            postText = Regex.Replace(postText, "https:\\/\\/(.*?) ", "", RegexOptions.Multiline);
            postText = Regex.Replace(postText, "https:\\/\\/(.*)$", "", RegexOptions.Multiline);
            postText = Regex.Replace(postText, "http:\\/\\/(.*?) ", "", RegexOptions.Multiline);
            postText = Regex.Replace(postText, "http:\\/\\/(.*)$", "", RegexOptions.Multiline);
            return postText;
        }

        /// <summary>
        /// Removes all already marked code as well as some Markdown content from the post body
        /// and then divides everything into a list of lines, which can be analyzed
        /// </summary>
        public static IEnumerable<string> PrepareBody(string body)
        {
            body = RemoveProperCode(body);
            body = RemoveMarkdownContent(body);

            return body.Replace("\r\n", "\n").Split("\n\n").Where(x => x.Trim() != "").Distinct().ToList();
        }
    }
}