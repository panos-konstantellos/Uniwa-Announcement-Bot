using System;

namespace Uniwa.Announcement.Bot
{
    public sealed class Article
    {
        public string Title { get; set; }
        
        public string Summary { get; set; }
        
        public string Link { get; set; }
        
        public DateTime PublishedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }
    }
}
