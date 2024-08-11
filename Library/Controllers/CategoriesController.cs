using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Library.Authorization;
using Library.DTOs;
using Serilog;
using Library.Repositories;
using Library.Services;

namespace Library.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class CategoriesController(
        ICategoryService categoryService) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            Log.Information("Getting list of categories.");
            var categories = await categoryService.GetCategoriesAsync();
            Log.Information("Retrieved {Count} categories.", categories.Count());
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            Log.Information("Getting category with ID {Id}.", id);
            var category = await categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            Log.Information("Retrieved category with ID {Id}.", id);
            return Ok(category);
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Category>> AddCategory(Category category)
        {
            Log.Information("Adding new category with Name {Name}.", category.Name);
            await categoryService.AddCategoryAsync(category);
            Log.Information("Category with ID {Id} added.", category.Id);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [CheckPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Log.Information("Updating category with ID {Id}.", id);
            await categoryService.UpdateCategoryAsync(id, categoryDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Log.Information("Deleting category with ID {Id}.", id);
            await categoryService.DeleteCategoryAsync(id);
            Log.Information("Category with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}