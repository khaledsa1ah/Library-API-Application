using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Day1.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Day1.Authorization;
using Serilog;
using Day1.Repositories;

namespace MyWebAPP.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class CategoriesController(
        ApplicationDbContext _context,
        IMemoryCache _cache,
        CategoryRepository _categoryRepository) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            Log.Information("GetAllCategories called at {Time}", DateTime.UtcNow);

            var cacheKey = "categories";
            if (!_cache.TryGetValue(cacheKey, out List<Category> categories))
            {
                categories = (await _categoryRepository.GetAllAsync()).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, categories, cacheOptions);
                Log.Information("Categories retrieved from database and cached at {Time}", DateTime.UtcNow);
            }
            else
            {
                Log.Information("Categories retrieved from cache at {Time}", DateTime.UtcNow);
            }

            return categories;
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Category>> AddCategory(Category category)
        {
            Log.Information("AddCategory called at {Time}", DateTime.UtcNow);

            await _categoryRepository.AddAsync(category);

            var cacheKey = "categories";
            _cache.Remove(cacheKey); // Invalidate cache

            Log.Information("Category added and cache invalidated at {Time}", DateTime.UtcNow);

            return CreatedAtAction(nameof(AddCategory), new { id = category.Id }, category);
        }

        [HttpDelete("{id}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Log.Information("DeleteCategory called for id {Id} at {Time}", id, DateTime.UtcNow);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await _categoryRepository.DeleteAsync(category);

            var cacheKey = $"category_{id}";
            _cache.Remove(cacheKey); // Invalidate cache for specific category
            _cache.Remove("categories"); // Invalidate cache for all categories

            Log.Information("Category with id {Id} deleted and cache invalidated at {Time}", id,
                DateTime.UtcNow);

            return NoContent();
        }
    }
}