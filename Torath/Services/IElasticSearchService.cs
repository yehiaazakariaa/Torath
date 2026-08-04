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

        // Requirement: Create the required Elasticsearch indices if they do not exist.
        Task CreateIndexIfNotExistsAsync();

        // Requirement: Update existing, avoid duplicates, and log failures.
        Task BulkIndexDocumentsAsync(IEnumerable<SearchDocument> documents);

        // The single unified search method using our new DTO
        Task<object> SearchAsync(SearchRequestDto request);
    }
}