using System;
using System.Collections.Generic;
using System.Text;

namespace Eleia.CoyoteApi
{

    public class TopicsApiResult
    {
        public Topic[] topics { get; set; }
        public Links links { get; set; }
        public Meta meta { get; set; }
    }    

    public class Topic
    {
        public int id { get; set; }
        public string subject { get; set; }
        public int score { get; set; }
        public int views { get; set; }
        public int replies { get; set; }
        public int is_sticky { get; set; }
        public int is_locked { get; set; }
        public object locked_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime last_post_created_at { get; set; }
        public string url { get; set; }
        public Forum forum { get; set; }
        public Tag[] tags { get; set; }
    }

    public class Forum
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Tag
    {
        public int id { get; set; }
        public string name { get; set; }
        public string real_name { get; set; }
        public string logo { get; set; }
        public string url { get; set; }
    }

}
