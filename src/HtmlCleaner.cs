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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Eleia
{
    /// <summary>
    /// Removes elements from HTML code of a post which are not needed by analyzers
    /// </summary>
    public static class HtmlCleaner
    {
        /// <summary>
        /// Removes properly formatted code from a post (with &lt;code&gt; tag)
        /// </summary>
        /// <param name="postText">Post content to be cleaned (in HTML)</param>
        /// <returns>Post content with Pre and Code elements removed</returns>
        public static string RemoveProperCode(string postText)
        {
            postText = Regex.Replace(postText, "<pre><code(.|\n)*?</pre>", "", RegexOptions.Multiline);
            postText = Regex.Replace(postText, "<code(.|\n)*?</code>", "", RegexOptions.Multiline);
            return postText;
        }

        /// <summary>
        /// Removes links to downloading attachements
        /// </summary>
        /// <param name="postText">Post content to be cleaned (in HTML)</param>
        /// <returns>Post content without links to post attachments</returns>
        public static string RemoveDownloadLinks(string postText)
        {
            postText = Regex.Replace(postText, "<i class=\"fa fa-download(.|\n)*?</li>", "", RegexOptions.Multiline);
            return postText;
        }

        /// <summary>
        /// Removes all HTML tags
        /// </summary>
        /// <param name="item">String with HTML tags</param>
        /// <returns>String without HTML tags</returns>
        public static string StripTags(string item)
        {
            return Regex.Replace(item, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Removes whitespace (\n, any space more than once) from string and trims it
        /// </summary>
        /// <param name="item">String with whitespace</param>
        /// <returns>Trimmed string without whitespace</returns>
        public static string StripWhitespace(string item)
        {
            var cleaned = item.Replace("\n", "");
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            return cleaned;
        }
    }
}