using System;
using Search.Dialogs;
using Search.Services;
using System.Threading.Tasks;

namespace KylieBot.Dialogs
{
    [Serializable]
    public class IndexSearchDialog : SearchDialog
    {
        private static readonly string[] TopRefiners = {  "Product", "Version", "Category", "Rating", "Source" };

        public IndexSearchDialog(ISearchClient searchClient) : base(searchClient, multipleSelection: false)
        {
        }

        protected override string[] GetTopRefiners()
        {
            return TopRefiners;
        }
    }
}