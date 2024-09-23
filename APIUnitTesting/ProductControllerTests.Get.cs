using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Interfaces;

namespace APIUnitTesting
{
    [TestClass]
    public partial class ProductControllerTests
    {
        private Mock<IProductRepository> _mockRepo;
        private ProductController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IProductRepository>();
            _controller = new ProductController(_mockRepo.Object);
        }

        [TestMethod]
        public async Task GetProductById_ReturnsOk_WhenProductExists()
        {
            var product = new Product { ProductID = 1, ProductName = "Test Product", Price_Value = 99.99M, Currency_Code = "USD" };
            _mockRepo.Setup(repo => repo.GetProductByIdAsync(1)).ReturnsAsync(product);

            var result = await _controller.GetProductById(1);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var returnValue = okResult.Value as Product;
            Assert.AreEqual(1, returnValue.ProductID);
        }

        [TestMethod]
        public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            _mockRepo.Setup(repo => repo.GetProductByIdAsync(1)).ReturnsAsync((Product)null);

            var result = await _controller.GetProductById(1);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task GetProductById_ReturnsAllProducts_WhenNoIdIsPassed()
        {
            var products = new List<Product>
            {
                new Product { ProductID = 1, ProductName = "Test Product 1", Price_Value = 99.99M, Currency_Code = "USD" },
                new Product { ProductID = 2, ProductName = "Test Product 2", Price_Value = 199.99M, Currency_Code = "EUR" }
            };
            _mockRepo.Setup(repo => repo.GetAllProductsAsync()).ReturnsAsync(products);

            var result = await _controller.GetProductById(null);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var returnValue = okResult.Value as List<Product>;
            Assert.AreEqual(2, returnValue.Count);
        }
    }
}