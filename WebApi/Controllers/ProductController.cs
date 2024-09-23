using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Models;
using System.Linq;
using WebApi.Interfaces;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductController(IProductRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertProducts([FromBody] List<ProductInputModel> productData)
        {
            if (productData == null || !productData.Any())
                return BadRequest("Product data is required");

            try
            {
                await _repository.InsertMultipleProductsAsync(productData);
                return Ok("Products inserted successfully");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id?}")]
        public async Task<IActionResult> GetProductById(int? id)
        {
            if (id.HasValue)
            {
                var product = await _repository.GetProductByIdAsync(id.Value);

                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found.");
                }

                return Ok(product);
            }
            else
            {
                var products = await _repository.GetAllProductsAsync();
                return Ok(products);
            }
        }
    }
}
