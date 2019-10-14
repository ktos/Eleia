using System;
using System.Collections.Generic;
using System.Text;

namespace Eleia.CoyoteApi
{
    public class PostsApiResult
    {
        public Post[] data { get; set; }
        public Links links { get; set; }
        public Meta meta { get; set; }
    }

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

    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string photo { get; set; }
    }
}