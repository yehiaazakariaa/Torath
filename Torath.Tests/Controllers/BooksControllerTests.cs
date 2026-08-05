using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Torath.Controllers;
using Torath.Services;
using Torath.Entities; // FIX: Added the Entities namespace

namespace Torath.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly BooksController _booksController;

        public BooksControllerTests()
        {
            _bookServiceMock = new Mock<IBookService>();
            _booksController = new BooksController(_bookServiceMock.Object);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk200_WhenBookExists()
        {
            // Arrange
            var bookId = 4001;

            // FIX: Using the Book entity instead of BookDto
            var expectedBook = new Book { Id = bookId, Title = "The Golden Age of Egypt" };

            _bookServiceMock.Setup(s => s.GetByIdAsync(bookId, default))
                            .ReturnsAsync(expectedBook);

            // Act
            var result = await _booksController.GetById(bookId, default);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedBook);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound404_WhenBookDoesNotExist()
        {
            // Arrange
            var invalidBookId = 9999;

            // FIX: Casting null to the Book entity
            _bookServiceMock.Setup(s => s.GetByIdAsync(invalidBookId, default))
                            .ReturnsAsync((Book)null);

            // Act
            var result = await _booksController.GetById(invalidBookId, default);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task GetAll_ShouldApplyPaginationAndFilters_WhenProvided()
        {
            // Arrange
            int expectedPage = 2;
            int expectedPageSize = 5;
            string filterCategory = "History";
            string filterLanguage = "Arabic";

            // Act
            var result = await _booksController.GetAll(expectedPage, expectedPageSize, filterCategory, filterLanguage, default);

            // Assert
            // 1. Ensure it returns a 200 OK
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            // 2. Verify Pagination and Filtering parameters were passed to the service perfectly
            _bookServiceMock.Verify(s => s.GetAllAsync(
                expectedPage,
                expectedPageSize,
                filterCategory,
                filterLanguage,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

   
        
    }
}