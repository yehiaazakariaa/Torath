using System.Threading.Tasks;
using Torath.DTOs;
using Torath.SearchModels;

namespace Torath.Services
{
    public interface IElasticSearchService
    {
        // Creates or Updates a document in the Elasticsearch index
        Task IndexDocumentAsync(SearchDocument document);

        // Deletes a document from the Elasticsearch index when it's removed from SQL
        Task DeleteDocumentAsync(string documentId);

        // The single unified search method using our new DTO
        Task<object> SearchAsync(SearchRequestDto request);
    }
}