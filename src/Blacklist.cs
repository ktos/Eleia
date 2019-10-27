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

using Eleia.CoyoteApi;
using System.Collections.Generic;
using System.Linq;

namespace Eleia
{
    /// <summary>
    /// Class for blacklisting certain forums
    /// </summary>
    public class Blacklist
    {
        private readonly HashSet<int> forums;

        /// <summary>
        /// Creates a new instance of blacklist based on a definition string
        /// </summary>
        public Blacklist()
        {
            forums = new HashSet<int>();
        }

        /// <summary>
        /// Updates a blacklist with a new definition string
        /// </summary>
        /// <param name="definition">A comma-separated list of forum ids</param>
        public void UpdateDefinition(string definition)
        {
            forums.UnionWith(definition.Split(',').Select(x => int.Parse(x)));
        }

        /// <summary>
        /// Returns if the specified post is disallowed by the blacklist
        /// </summary>
        /// <param name="post">Post to be checked against the blacklist</param>
        /// <returns>Returns true if post is forbidden, false if it is allowed</returns>
        public bool IsDisallowed(Post post)
        {
            return forums.Contains(post.forum_id);
        }
    }
}