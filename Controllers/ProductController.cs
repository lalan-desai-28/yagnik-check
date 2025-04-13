using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.IRepositories;
using ProductCatalog.Models;

namespace Task8Indentity.Controllers
{

    /// <summary>
    /// Controller for managing products in the catalog.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository) : ControllerBase
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;


        /// <summary>
        /// Get all products from the catalog.
        /// </summary>
        /// <returns>A list of all products.</returns>
        /// <response code="200">Returns the list of products.</response>
        /// <response code="400">Bad request if any issue occurs.</response>
        [HttpGet]
        [AllowAnonymous] // Anyone can view products
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productRepository.GetAll();
                return Ok(products);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logger)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific product by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <returns>The product with the specified ID.</returns>
        /// <response code="200">Returns the product details.</response>
        /// <response code="404">Product not found if no product matches the ID.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(string id)
        {
            try
            {
                var product = await _productRepository.GetById(id);
                if (product == null)
                    return NotFound("Product not found.");
                return Ok(product);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all products in a specific category.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category.</param>
        /// <returns>A list of products in the specified category.</returns>
        /// <response code="200">Returns the list of products in the category.</response>
        /// <response code="404">If the category does not exist or no products are found for that category.</response>
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByCategory(string categoryId)
        {
            try
            {
                var products = await _productRepository.GetByCategory(categoryId);
                if (products == null || !products.Any())
                    return NotFound("No products found in this category.");
                return Ok(products);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new product in the catalog.
        /// </summary>
        /// <param name="product">The product data to be created.</param>
        /// <returns>The created product.</returns>
        /// <response code="201">Returns the created product.</response>
        /// <response code="400">If the Category ID is invalid.</response>
        /// <response code="403">Forbidden if the user doesn't have the right role (Admin, Manager).</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                var category = await _categoryRepository.GetById(product.CategoryId);
                if (category == null)
                    return BadRequest("Invalid Category ID.");

                await _productRepository.Add(product);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing product in the catalog.
        /// </summary>
        /// <param name="id">The unique identifier of the product to be updated.</param>
        /// <param name="product">The updated product data.</param>
        /// <returns>Returns no content if the product was updated successfully.</returns>
        /// <response code="204">Product updated successfully.</response>
        /// <response code="400">If the Category ID is invalid or the product details are incorrect.</response>
        /// <response code="404">Product not found if no product matches the ID.</response>
        /// <response code="403">Forbidden if the user doesn't have the right role (Admin, Manager).</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
        {
            try
            {
                var existingProduct = await _productRepository.GetById(id);
                if (existingProduct == null)
                    return NotFound("Product not found.");

                var category = await _categoryRepository.GetById(product.CategoryId);
                if (category == null)
                    return BadRequest("Invalid Category ID.");

                product.Id = id;
                await _productRepository.Update(product);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a product from the catalog.
        /// </summary>
        /// <param name="id">The unique identifier of the product to be deleted.</param>
        /// <returns>Returns no content if the product was deleted successfully.</returns>
        /// <response code="204">Product deleted successfully.</response>
        /// <response code="404">Product not found if no product matches the ID.</response>
        /// <response code="403">Forbidden if the user doesn't have the right role (Admin, Manager).</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                var existingProduct = await _productRepository.GetById(id);
                if (existingProduct == null)
                    return NotFound("Product not found.");

                await _productRepository.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }

}
