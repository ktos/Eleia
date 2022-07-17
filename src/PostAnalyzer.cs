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

        public class TextAnalysisResult
        {
            public bool Prediction { get; set; }
            public double Probability { get; set; }

            public string Item { get; set; }
        }

        /// <summary>
        /// Analyzes a single post in search of problems, based on text (not HTML) content
        /// </summary>
        /// <param name="post">Post to be analyzed, in the form of object from the API</param>
        /// <returns>List of possible problems found, with their probabilities</returns>
        public TextAnalysisResult AnalyzeText(CoyoteApi.Post post)
        {
            var output = new List<PostProblems>();
            var body = TextCleaner.PrepareBody(post.text);

            foreach (var item in body)
            {
                var predictionResult = codeDetector.Predict(item);

                if (predictionResult.Prediction && predictionResult.Probability > codeDetectorTreshold)
                {
                    return new TextAnalysisResult { Prediction = true, Probability = predictionResult.Probability, Item = item };
                }
            }

            return new TextAnalysisResult { Prediction = false };
        }
    }
}