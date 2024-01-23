using System.Collections.Generic;
using System.Threading.Tasks;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch.Models
{

    public class SearchResultsModel
    {
        public IEnumerable<SearchResultModel> OrderResults { get; set; }

        public IEnumerable<SearchResultModel> CustomerResults { get; set; }

        public IEnumerable<SearchResultModel> ProductResults { get; set; }

        public SearchResultsModel() { }

        public SearchResultsModel(IEnumerable<SearchResultModel> orderResults, IEnumerable<SearchResultModel> customerResults, IEnumerable<SearchResultModel> productResults)
        {
            OrderResults = orderResults;
            CustomerResults = customerResults;
            ProductResults = productResults;
        }
    }

    public class SearchResultModel
    {
        public int EntityId { get; set; }

        public string ResultTitle { get; set; }

        public string ResultUrl { get; set; }

        public int RelevanceScore { get; set; }

        public SearchResultModel() { }

        public SearchResultModel(int entityId, string resultTitle, string resultUrl, int relevanceScore = 0)
        {
            EntityId = entityId;
            ResultTitle = resultTitle;
            ResultUrl = resultUrl;
            RelevanceScore = relevanceScore;
        }
    }
}
