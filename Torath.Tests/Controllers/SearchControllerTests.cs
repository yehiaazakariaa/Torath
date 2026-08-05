using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Torath.Controllers;
using Torath.Services;
using Torath.DTOs;

namespace Torath.Tests.Controllers
{
    public class SearchControllerTests
    {
        private readonly Mock<IElasticSearchService> _elasticSearchServiceMock;
        private readonly SearchController _searchController;

        public SearchControllerTests()
        {
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();
            _searchController = new SearchController(_elasticSearchServiceMock.Object);
        }

        [Fact]
        public async Task Search_ShouldReturnOk200_WhenQueryIsValid()
        {
            // Arrange
            var searchRequest = new SearchRequestDto
            {
                Query = "Ancient Egypt",
                PageNumber = 1, // FIX: Changed from Page to PageNumber
                PageSize = 10
            };

            // Mock search response payload
            var mockSearchResult = new
            {
                Total = 1,
                Data = new[]
                {
                    new { Id = "Book_4001", Title = "The Golden Age of Egypt", ContentType = "Book" }
                }
            };

            _elasticSearchServiceMock
                .Setup(s => s.SearchAsync(searchRequest))
                .ReturnsAsync(mockSearchResult);

            // Act
            var result = await _searchController.Search(searchRequest);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mockSearchResult);
        }

        [Fact]
        public async Task Search_ShouldPassSearchRequestToElasticService_Once()
        {
            // Arrange
            var searchRequest = new SearchRequestDto
            {
                Query = "Manuscripts",
                CategoryId = 4, // FIX: Changed from string 'Category' to int 'CategoryId'
                PageNumber = 1, // FIX: Changed from Page to PageNumber
                PageSize = 5
            };

            // Act
            await _searchController.Search(searchRequest);

            // Assert
            _elasticSearchServiceMock.Verify(s => s.SearchAsync(searchRequest), Times.Once);
        }
    }
}