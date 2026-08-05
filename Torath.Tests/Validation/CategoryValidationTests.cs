using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Torath.DTOs;

namespace Torath.Tests.Validation
{
    public class CategoryValidationTests
    {
        [Fact]
        public void CategoryWriteDto_ShouldFailValidation_WhenNameIsMissing()
        {
            // Arrange
            // We intentionally leave the Name blank to trigger a validation error
            var invalidDto = new CategoryWriteDto { Name = "" };

            var validationContext = new ValidationContext(invalidDto);
            var validationResults = new List<ValidationResult>();

            // Act
            // This simulates what ASP.NET automatically does when a request comes in
            var isValid = Validator.TryValidateObject(invalidDto, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse(); // It should fail
            validationResults.Should().NotBeEmpty(); // It should generate an error message
        }

        [Fact]
        public void CategoryWriteDto_ShouldPassValidation_WhenNameIsProvided()
        {
            // Arrange
            var validDto = new CategoryWriteDto { Name = "Science Fiction" };
            var validationContext = new ValidationContext(validDto);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(validDto, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue(); // It should pass perfectly
            validationResults.Should().BeEmpty();
        }
    }
}