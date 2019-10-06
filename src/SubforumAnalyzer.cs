using System;
using System.Collections.Generic;
using System.Text;

namespace Eleia
{
    internal static class SubforumAnalyzer
    {
        public static List<string> GetTopics(string html)
        {
            var output = new List<string>();
            var hap = new HtmlAgilityPack.HtmlDocument();
            hap.LoadHtml(html);

            var topics = hap.DocumentNode.SelectNodes("//h5/a");

            foreach (var item in topics)
            {
                output.Add(item.Attributes["href"].Value);
            }

            return output;
        }
    }
}