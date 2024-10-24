// Tests/ProductControllerTests.cs
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Projekt1.Controllers;
using Projekt1.Models;
using Microsoft.AspNetCore.Mvc;
using Projekt1.Models;

namespace Projekt1.Tests
{
    

    public class ProductControllerTests
    {
        private ProductController GetProductControllerWithMockDb(out Mock<ApplicationDbContext> mockDb)
        {
            mockDb = new Mock<ApplicationDbContext>(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb").Options);

            return new ProductController(mockDb.Object);
        }

        [Fact]
        public async Task PostProduct_ValidProduct_ReturnsCreatedResult()
        {
            // Arrange
            var controller = GetProductControllerWithMockDb(out var mockDb);
            var newProduct = new Product
            {
                Name = "TestProduct",
                Price = 10.0m,
                StockQuantity = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Product>(createdResult.Value);
            Assert.Equal("TestProduct", returnValue.Name);
        }

        [Fact]
        public async Task PostProduct_InvalidPrice_ReturnsBadRequest()
        {
            // Arrange
            var controller = GetProductControllerWithMockDb(out var mockDb);
            var newProduct = new Product
            {
                Name = "InvalidProduct",
                Price = -5.0m,  // Invalid price
                StockQuantity = 10
            };

            // Act
            var result = await controller.PostProduct(newProduct);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Cena produktu musi być większa niż 0,01 PLN.", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task PutProduct_StockQuantityZero_StatusChangesToUnavailable()
        {
            // Arrange
            var controller = GetProductControllerWithMockDb(out var mockDb);
            var existingProduct = new Product
            {
                Id = 1,
                Name = "Product1",
                Price = 10.0m,
                StockQuantity = 5
            };
            mockDb.Object.Products.Add(existingProduct);
            await mockDb.Object.SaveChangesAsync();

            var updatedProduct = new Product
            {
                Id = 1,
                Name = "Product1",
                Price = 10.0m,
                StockQuantity = 0  // Stock set to 0
            };

            // Act
            await controller.PutProduct(1, updatedProduct);
            var result = await controller.GetProduct(1);

            // Assert
            var productResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.Equal("Niedostępny", productResult.Value.Status);
        }

        [Fact]
        public async Task GetProductHistory_ReturnsChangeLog()
        {
            // Arrange
            var controller = GetProductControllerWithMockDb(out var mockDb);
            var existingProduct = new Product
            {
                Id = 1,
                Name = "Product1",
                Price = 10.0m,
                StockQuantity = 5,
                ChangeHistory = new List<ChangeLog>
        {
            new ChangeLog { ChangeDescription = "Utworzono produkt." },
            new ChangeLog { ChangeDescription = "Zaktualizowano ilość produktów." }
        }
            };
            mockDb.Object.Products.Add(existingProduct);
            await mockDb.Object.SaveChangesAsync();

            // Act
            var result = await controller.GetProductHistory(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var changeLogs = Assert.IsType<List<ChangeLog>>(okResult.Value);
            Assert.Equal(2, changeLogs.Count);
        }
    }
}
