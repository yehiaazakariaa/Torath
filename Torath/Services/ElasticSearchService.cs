using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic; // Added for IEnumerable and List
using System.Threading.Tasks;
using Torath.SearchModels;

namespace Torath.Services
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ILogger<ElasticSearchService> _logger;
        private const string IndexName = "torath-searchable-content"; // Unified search index

        public ElasticSearchService(ElasticsearchClient client, ILogger<ElasticSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task IndexDocumentAsync(SearchDocument document)
        {
            try
            {
                // Adds or updates the document in Elasticsearch (Upsert)
                var response = await _client.IndexAsync(document, idx => idx
                    .Index(IndexName)
                    .Id(document.Id)
                );

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Failed to index document {Id} in Elasticsearch: {Reason}", document.Id, response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while indexing document {Id}", document.Id);
            }
        }

        public async Task DeleteDocumentAsync(string documentId)
        {
            try
            {
                // Removes document from Elasticsearch when deleted from SQL
                var response = await _client.DeleteAsync<SearchDocument>(documentId, d => d.Index(IndexName));

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Failed to delete document {Id} from Elasticsearch: {Reason}", documentId, response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting document {Id}", documentId);
            }
        }

        public async Task<IEnumerable<SearchDocument>> SearchAsync(string keyword)
        {
            try
            {
                // Return empty list early for blank queries
                if (string.IsNullOrWhiteSpace(keyword))
                    return new List<SearchDocument>();

                // MultiMatch search across Title, Description, Author, Keywords, and Content
                var response = await _client.SearchAsync<SearchDocument>(s => s
                    .Index(IndexName)
                    .Query(q => q
                        .MultiMatch(m => m
                            .Query(keyword)
                            .Fields(new[] { "title^3", "description", "author^2", "keywords", "content" })
                        )
                    )
                );

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Elasticsearch search failed: {Reason}", response.DebugInformation);
                    return new List<SearchDocument>();
                }

                return response.Documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching for {Keyword}", keyword);
                return new List<SearchDocument>();
            }
        }
    }
}