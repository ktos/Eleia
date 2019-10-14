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
    public static class HtmlCleaner
    {
        public static string RemoveProperCode(string postText)
        {
            // removing every code properly put in the <code> or <pre><code class=""> tags
            postText = Regex.Replace(postText, "<pre><code(.|\n)*?</pre>", "", RegexOptions.Multiline);
            postText = Regex.Replace(postText, "<code(.|\n)*?</code>", "", RegexOptions.Multiline);

            // removing every link to attachment download
            postText = Regex.Replace(postText, "<i class=\"fa fa-download(.|\n)*?</li>", "", RegexOptions.Multiline);
            return postText;
        }

        public static List<string> CleanParagraphs(List<string> paras)
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