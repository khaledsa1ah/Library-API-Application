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
        UnitOfWork _unitOfWork,
        IMemoryCache _cache) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            Log.Information("GetCategories called at {Time}", DateTime.UtcNow);

            var cacheKey = "categories";
            if (!_cache.TryGetValue(cacheKey, out List<Category> categories))
            {
                categories = (await _unitOfWork.Categories.GetAllAsync()).ToList();

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

        [HttpGet("{id}")]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            Log.Information("GetCategoryById called with id {Id} at {Time}", id, DateTime.UtcNow);

            var cacheKey = $"category_{id}";
            if (!_cache.TryGetValue(cacheKey, out Category category))
            {
                category = await _unitOfWork.Categories.GetByIdAsync(id);

                if (category == null)
                {
                    Log.Warning("Category with id {Id} not found at {Time}", id, DateTime.UtcNow);
                    return NotFound();
                }

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, category, cacheOptions);
                Log.Information("Category with id {Id} retrieved from database and cached at {Time}", id,
                    DateTime.UtcNow);
            }
            else
            {
                Log.Information("Category with id {Id} retrieved from cache at {Time}", id, DateTime.UtcNow);
            }

            return category;
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Category>> AddCategory(Category category)
        {
            Log.Information("AddCategory called at {Time}", DateTime.UtcNow);

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync(); // Commit changes

            var cacheKey = "categories";
            _cache.Remove(cacheKey); // Invalidate cache

            Log.Information("Category added and cache invalidated at {Time}", DateTime.UtcNow);

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [CheckPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            Log.Information("UpdateCategory called for id {Id} at {Time}", id, DateTime.UtcNow);

            try
            {
                await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync(); // Commit changes

                var cacheKey = $"category_{id}";
                _cache.Remove(cacheKey); // Invalidate cache for specific category
                _cache.Remove("categories"); // Invalidate cache for all categories

                Log.Information("Category with id {Id} updated and cache invalidated at {Time}", id,
                    DateTime.UtcNow);
            }
            catch (Exception)
            {
                if (!await CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Log.Information("DeleteCategory called for id {Id} at {Time}", id, DateTime.UtcNow);

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await _unitOfWork.Categories.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync(); // Commit changes

            var cacheKey = $"category_{id}";
            _cache.Remove(cacheKey); // Invalidate cache for specific category
            _cache.Remove("categories"); // Invalidate cache for all categories

            Log.Information("Category with id {Id} deleted and cache invalidated at {Time}", id, DateTime.UtcNow);

            return NoContent();
        }

        private async Task<bool> CategoryExists(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            return category != null;
        }
    }
}