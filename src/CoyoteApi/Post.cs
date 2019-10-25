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

namespace Eleia.CoyoteApi
{
#pragma warning disable IDE1006 // Naming Styles

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PostsApiResult
    {
        public IEnumerable<Post> data { get; set; }
        public Links links { get; set; }
        public Meta meta { get; set; }
    }

    public class SinglePostApiResult
    {
        public SinglePost data { get; set; }
    }

    public class SinglePost
    {
        public int id { get; set; }
        public object user_name { get; set; }
        public int score { get; set; }
        public int edit_count { get; set; }
        public int forum_id { get; set; }
        public int topic_id { get; set; }
        public DateTime created_at { get; set; }
        public User user { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public Comment[] comments { get; set; }
    }

    public class Comment
    {
        public int id { get; set; }
        public int post_id { get; set; }
        public int user_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object deleted_at { get; set; }
        public string text { get; set; }
        public User user { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Post
    {
        public int id { get; set; }
        public object user_name { get; set; }
        public int score { get; set; }
        public int edit_count { get; set; }
        public int forum_id { get; set; }
        public int topic_id { get; set; }
        public DateTime created_at { get; set; }
        public User user { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string photo { get; set; }
    }

#pragma warning restore IDE1006 // Naming Styles
}