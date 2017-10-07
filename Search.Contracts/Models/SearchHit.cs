namespace Search.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class SearchHit
    {
        public SearchHit()
        {
            this.PropertyBag = new Dictionary<string, object>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string[] Keywords { get; set; }
        public string Content { get; set; }
        public string Rating { get; set; }
        public int NumberOfRatings { get; set; }
        public int TotalRatingScore { get; set; }
        public string[] Tags { get; set; }
        public string Product { get; set; }
        public string Version { get; set; }
        public string Category { get; set; }
        public string Source { get; set; }
        public string SourceLink { get; set; }
        public DateTime LoadDate { get; set; }
        public DateTime ArticleDate { get; set; }
        public int MinorVersionNumber { get; set; }
        public int MajorVersionNumber { get; set; }

        public IDictionary<string, object> PropertyBag { get; set; }
    }
}