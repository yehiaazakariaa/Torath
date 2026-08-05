using Moq;
using FluentAssertions;
using Torath.Entities; // FIX 1: Pointing to your actual Entities folder
using Torath.Repositories;
using Torath.Services;
using Torath.DTOs;


namespace Torath.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<IRepository<Category>>();
            _categoryService = new CategoryService(_categoryRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var expectedCategory = new Category { Id = categoryId, Name = "History", Description = "Ancient texts" };

            // FIX 2: Added It.IsAny<CancellationToken>() so Moq knows how to match the signature
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(expectedCategory);

            // Act
            // FIX 3: Passed 'default' as the cancellation token to the service call
            var result = await _categoryService.GetByIdAsync(categoryId, default);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.Name.Should().Be("History");
        }

        [Fact]

        public async Task CreateAsync_ShouldCallRepositoryAdd_WhenValidCategoryProvided()
        {
            // Arrange
            // FIX: Create a DTO instead of a database Entity
            var newCategoryDto = new CategoryWriteDto { Name = "Science" };

            // Act
            // Pass the DTO to your service
            await _categoryService.CreateAsync(newCategoryDto, default);

            // Assert
            // The service should map the DTO to an entity behind the scenes, 
            // so we still verify that the repository's AddAsync was called with a Category.
            _categoryRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}