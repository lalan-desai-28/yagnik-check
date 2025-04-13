using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.IRepositories;
using ProductCatalog.Models;

namespace Task8Indentity.Controllers
{

    /// <summary>
    /// Controller for managing categories in the product catalog.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class CategoryController(ICategoryRepository categoryRepository) : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;

        /// <summary>
        /// Get all categories in the catalog.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        /// <response code="200">Returns the list of categories.</response>
        /// <response code="400">Bad request if any issue occurs.</response>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetAll();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logger)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific category by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>The category with the specified ID.</returns>
        /// <response code="200">Returns the category details.</response>
        /// <response code="404">Category not found if no category matches the ID.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            try
            {
                var category = await _categoryRepository.GetById(id);
                if (category == null)
                    return NotFound("Category not found.");
                return Ok(category);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new category in the catalog.
        /// </summary>
        /// <param name="category">The category data to be created.</param>
        /// <returns>The created category.</returns>
        /// <response code="201">Returns the created category.</response>
        /// <response code="400">If the category already exists by name.</response>
        /// <response code="403">Forbidden if the user doesn't have the right role (Admin, Manager).</response>
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (await _categoryRepository.GetByName(category.Name) != null)
                    return BadRequest("Category already exists.");

                await _categoryRepository.Add(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing category in the catalog.
        /// </summary>
        /// <param name="id">The unique identifier of the category to be updated.</param>
        /// <param name="category">The updated category data.</param>
        /// <returns>Returns no content if the category was updated successfully.</returns>
        /// <response code="204">Category updated successfully.</response>
        /// <response code="404">Category not found if no category matches the ID.</response>
        /// <response code="403">Forbidden if the user doesn't have the right role (Admin, Manager).</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] Category category)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetById(id);
                if (existingCategory == null)
                    return NotFound("Category not found.");

                category.Id = id;
                await _categoryRepository.Update(category);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a category from the catalog.
        /// </summary>
        /// <param name="id">The unique identifier of the category to be deleted.</param>
        /// <returns>Returns no content if the category was deleted successfully.</returns>
        /// <response code="204">Category deleted successfully.</response>
        /// <response code="404">Category not found if no category matches the ID.</response>
        /// <response code="403">Forbidden if the user doesn't have the right role (Admin, Manager).</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetById(id);
                if (existingCategory == null)
                    return NotFound("Category not found.");

                await _categoryRepository.Delete(id);
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
