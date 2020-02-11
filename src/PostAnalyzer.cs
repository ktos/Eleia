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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Eleia
{
    /// <summary>
    /// Analyzes the post on the Coyote forum in search of problems
    /// </summary>
    public class PostAnalyzer
    {
        private readonly float codeDetectorTreshold = 0.99f;
        private readonly CodeDetector codeDetector;

        /// <summary>
        /// Creates a new instance of PostAnalyzer, loads all detectors used
        /// in analyze process
        /// </summary>
        public PostAnalyzer(IConfigurationRoot configuration)
        {
            if (configuration != null)
            {
                codeDetectorTreshold = configuration.GetValue("threshold", 0.99f);
            }

            codeDetector = new CodeDetector();
        }

        /// <summary>
        /// Analyzes a single post in search of problems
        /// </summary>
        /// <param name="post">Post to be analyzed, in the form of object from the API</param>
        /// <returns>List of possible problems found, with their probabilities</returns>
        public List<PostProblems> Analyze(CoyoteApi.Post post)
        {
            var output = new List<PostProblems>();
            var unformatted = CheckForUnformattedCode(post);

            if (unformatted != null) output.Add(unformatted);

            return output;
        }

        private NotFormattedCodeFound CheckForUnformattedCode(CoyoteApi.Post post)
        {
            var text = HtmlCleaner.RemoveProperCode(post.html);
            text = HtmlCleaner.RemoveDownloadLinks(text);
            text = HtmlCleaner.RemoveHTMLContent(text);

            foreach (var para in text.Split("</p>").Select(CleanParagraph))
            {
                var result = codeDetector.Predict(para);

                if (result.Prediction && result.Probability > codeDetectorTreshold)
                {
                    return new NotFormattedCodeFound { Probability = result.Probability, Paragraph = para };
                }
            }

            return null;
        }

        private string CleanParagraph(string item)
        {
            var cleaned = HtmlCleaner.StripTags(item);
            return HtmlCleaner.StripWhitespace(cleaned);
        }
    }
}