using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIUnitTesting
{
    public partial class ProductControllerTests
    {
        [TestMethod]
        public async Task InsertProducts_ReturnsOk_WhenProductsAreInsertedSuccessfully()
        {
            var productData = new List<ProductInputModel>
            {
                new ProductInputModel { ProductName = "Test Product A", Price_Value = 29.99M, Currency_Code = "USD" },
                new ProductInputModel { ProductName = "Test Product B", Price_Value = 19.99M, Currency_Code = "EUR" }
            };

            _mockRepo.Setup(repo => repo.InsertMultipleProductsAsync(It.IsAny<List<ProductInputModel>>()))
                     .Returns(Task.CompletedTask);

            var result = await _controller.InsertProducts(productData);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult");
            Assert.AreEqual("Products inserted successfully", okResult.Value);
        }

        [TestMethod]
        public async Task InsertProducts_ReturnsBadRequest_WhenNoProductDataIsProvided()
        {
            List<ProductInputModel> productData = null;

            var result = await _controller.InsertProducts(productData);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }
    }
}