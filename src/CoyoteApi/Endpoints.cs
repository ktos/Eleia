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

namespace Eleia.CoyoteApi
{
    public static class Endpoints
    {
        /// <summary>
        /// Gets or sets if dev version of 4programmers.net is used or the production one
        /// </summary>
        public static bool IsDebug { get; set; }

        /// <summary>
        /// Coyote API endpoint for getting new posts
        /// </summary>
        public static string PostsApi => IsDebug ? "http://api.dev.4programmers.info/v1/posts" : "http://api.4programmers.net/v1/posts";

        /// <summary>
        /// Coyote URL for main page
        /// </summary>
        public static string MainPage => IsDebug ? "http://dev.4programmers.info/" : "https://4programmers.net";

        /// <summary>
        /// Coyote URL for login page
        /// </summary>
        public static string LoginPage => IsDebug ? "http://dev.4programmers.info/Login" : "https://4programmers.net/Login";

        /// <summary>
        /// Coyote URL for comment to a post link
        /// </summary>
        public static string CommentPage => IsDebug ? "http://dev.4programmers.info/Forum/Comment" : "https://4programmers.net/Forum/Comment";

        /// <summary>
        /// Coyote URL to logout link
        /// </summary>
        public static string LogoutPage => IsDebug ? "http://dev.4programmers.info/Logout" : "https://4programmers.net/Logout";
    }
}