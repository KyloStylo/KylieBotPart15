using Search.Models;
using System.Linq;
using Search.Azure.Services;
using Microsoft.Azure.Search.Models;
using System;

namespace KylieBot.Helpers
{
    public class IndexMapper : IMapper<DocumentSearchResult, GenericSearchResult>
    {
        public GenericSearchResult Map(DocumentSearchResult documentSearchResult)
        {
            var searchResult = new GenericSearchResult();

            searchResult.Results = documentSearchResult.Results.Select(r => ToSearchHit(r)).ToList();
            searchResult.Facets = documentSearchResult.Facets?.ToDictionary(kv => kv.Key, kv => kv.Value.Select(f => ToFacet(f)));

            return searchResult;
        }

        private static GenericFacet ToFacet(FacetResult facetResult)
        {
            return new GenericFacet
            {
                Value = facetResult.Value,
                Count = facetResult.Count.Value
            };
        }

        private static SearchHit ToSearchHit(SearchResult hit)
        {
            return new SearchHit
            {
                Id = (string)hit.Document["id"],
                Title = (string)hit.Document["Title"],
                Keywords = (string[])hit.Document["Keywords"],
                Content = (string)hit.Document["Content"],
                Rating = hit.Document["Rating"] == null ? "" : (string)hit.Document["Rating"],
                NumberOfRatings = hit.Document["NumberOfRatings"] == null ? 0 : (int)hit.Document["NumberOfRatings"],
                TotalRatingScore = hit.Document["TotalRatingScore"] == null ? 0 : (int)hit.Document["TotalRatingScore"],
                Tags = (string[])hit.Document["Tags"],
                Product = (string)hit.Document["Product"],
                Version = (string)hit.Document["Version"],
                Category = (string)hit.Document["Category"],
                Source = (string)hit.Document["Source"],
                SourceLink = (string)hit.Document["SourceLink"],
                LoadDate = ((DateTimeOffset)hit.Document["LoadDate"]).DateTime,
                ArticleDate = ((DateTimeOffset)hit.Document["ArticleDate"]).DateTime,
                MinorVersionNumber = int.Parse(hit.Document["MinorVersionNumber"].ToString()),
                MajorVersionNumber = int.Parse(hit.Document["MajorVersionNumber"].ToString())
            };
        }
    }
}