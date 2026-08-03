using System.Threading.Tasks;
using Torath.SearchModels;

namespace Torath.Services
{
    public interface IElasticSearchService
    {
        // Creates or Updates a document in the Elasticsearch index
        Task IndexDocumentAsync(SearchDocument document);

        // Deletes a document from the Elasticsearch index when it's removed from SQL
        Task DeleteDocumentAsync(string documentId);

        Task<IEnumerable<SearchDocument>> SearchAsync(string keyword);
    }
}