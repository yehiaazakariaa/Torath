using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search; // Added for v8 Highlighting & Sorting
using Microsoft.Extensions.Logging;
using Torath.DTOs;
using Torath.SearchModels;

namespace Torath.Services
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ILogger<ElasticSearchService> _logger;
        private const string IndexName = "torath-searchable-content";

        public ElasticSearchService(ElasticsearchClient client, ILogger<ElasticSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task IndexDocumentAsync(SearchDocument document)
        {
            try
            {
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

        public async Task<object> SearchAsync(SearchRequestDto request)
        {
            // 1. Build dynamic filters (v8 syntax)
            var filters = new List<Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<SearchDocument>>>();

            if (!string.IsNullOrEmpty(request.ContentType))
                filters.Add(q => q.Match(m => m.Field(f => f.ContentType).Query(request.ContentType)));

            if (request.CategoryId.HasValue)
                filters.Add(q => q.Term(t => t.Field(f => f.CategoryId).Value(request.CategoryId.Value)));

            if (!string.IsNullOrEmpty(request.Language))
                filters.Add(q => q.Match(m => m.Field(f => f.Language).Query(request.Language)));

            if (request.PublicationDateFrom.HasValue || request.PublicationDateTo.HasValue)
            {
                // Note the added .Range(r => r.DateRange(...)) wrapper here!
                filters.Add(q => q.Range(r => r.DateRange(d =>
                {
                    d.Field(f => f.PublicationDate);
                    if (request.PublicationDateFrom.HasValue) d.Gte(request.PublicationDateFrom.Value);
                    if (request.PublicationDateTo.HasValue) d.Lte(request.PublicationDateTo.Value);
                })));
            }

            // Prepare the main Must query cleanly
            Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<SearchDocument>> mustQuery =
                string.IsNullOrWhiteSpace(request.Query)
                    ? m => m.MatchAll()
                    : m => m.MultiMatch(mm => mm
                        .Query(request.Query)
                        .Fields(new[] { "title^2", "description", "content", "author", "keywords" })
                        .Fuzziness(new Fuzziness("AUTO"))
                    );

            // 2. Execute the Search on Elasticsearch
            var searchResponse = await _client.SearchAsync<SearchDocument>(s => s
                .Indices(IndexName) // Fixed the obsolete warning!
                .From((request.PageNumber - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q
                    .Bool(b => b
                        .Must(mustQuery)
                        .Filter(filters.ToArray())
                    )
                )
                .Sort(srt =>
                {
                    if (string.IsNullOrEmpty(request.SortBy))
                    {
                        // Default to relevance score
                        srt.Score(sc => sc.Order(SortOrder.Desc));
                    }
                    else
                    {
                        // Automatically append .keyword if sorting by text fields like Title or Author
                        var sortField = request.SortBy;
                        if (sortField.Equals("title", StringComparison.OrdinalIgnoreCase) ||
                            sortField.Equals("author", StringComparison.OrdinalIgnoreCase) ||
                            sortField.Equals("contentType", StringComparison.OrdinalIgnoreCase))
                        {
                            sortField = $"{request.SortBy}.keyword";
                        }

                        srt.Field(sortField, f => f.Order(request.SortDescending ? SortOrder.Desc : SortOrder.Asc));
                    }
                })
                .Highlight(h => h
                    .PreTags(new[] { "<mark>" })
                    .PostTags(new[] { "</mark>" })
                    .Fields(fs => fs
                        .Add("title", new HighlightField())
                        .Add("description", new HighlightField())
                        .Add("content", new HighlightField())
                        .Add("author", new HighlightField())   
                        .Add("keywords", new HighlightField())
                    )
                )
            );

            if (!searchResponse.IsValidResponse)
            {
                throw new Exception($"Elasticsearch search failed: {searchResponse.DebugInformation}");
            }

            // 3. Structure the final result
            return new
            {
                TotalResults = searchResponse.Total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Results = searchResponse.Hits.Select(hit => new
                {
                    ContentType = hit.Source?.ContentType,
                    Id = hit.Source?.Id,
                    DatabaseId = hit.Source.DatabaseId,
                    Title = hit.Source?.Title,
                    Description = hit.Source?.Description,
                    CategoryId = hit.Source?.CategoryId,
                    Language = hit.Source?.Language,
                    PublicationDate = hit.Source?.PublicationDate,
                    RelevanceScore = hit.Score,
                    Highlights = hit.Highlight
                })
            };
        }



        public async Task CreateIndexIfNotExistsAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);

            if (!existsResponse.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync(IndexName);

                if (!createResponse.IsValidResponse)
                {
                    _logger.LogError("Failed to create Elasticsearch index '{IndexName}': {Reason}", IndexName, createResponse.DebugInformation);
                    throw new Exception($"Could not create index {IndexName}");
                }

                _logger.LogInformation("Successfully created Elasticsearch index '{IndexName}'.", IndexName);
            }
        }

        public async Task BulkIndexDocumentsAsync(IEnumerable<SearchDocument> documents)
        {
            if (documents == null || !documents.Any()) return;

            // We use 'UpdateMany' with 'DocAsUpsert(true)' to fulfill two requirements:
            // 1. Update existing documents when necessary.
            // 2. Avoid duplicate documents (by matching on the exact ID).
            var bulkResponse = await _client.BulkAsync(b => b
                .Index(IndexName)
                .UpdateMany(documents, (descriptor, doc) => descriptor
                    .Id(doc.Id)
                    .Doc(doc)
                    .DocAsUpsert(true)
                )
            );

            // Requirement: Log failures.
            if (!bulkResponse.IsValidResponse || bulkResponse.Errors)
            {
                _logger.LogError("Bulk indexing encountered errors. Debug Info: {Info}", bulkResponse.DebugInformation);

                if (bulkResponse.ItemsWithErrors != null)
                {
                    foreach (var itemWithError in bulkResponse.ItemsWithErrors)
                    {
                        _logger.LogError("Failed to sync document {Id}: {Error}", itemWithError.Id, itemWithError.Error);
                    }
                }
            }
            else
            {
                _logger.LogInformation("Successfully synced {Count} documents to Elasticsearch.", documents.Count());
            }
        }
    }
}