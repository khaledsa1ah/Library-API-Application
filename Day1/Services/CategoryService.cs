using Day1.DTOs;

namespace Day1.Services;

using Data;
using Microsoft.Extensions.Caching.Memory;
using Repositories;
using Serilog;

public class CategoryService(UnitOfWork _unitOfWork, IMemoryCache _cache) : ICategoryService
{
    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
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

    public async Task<Category> GetCategoryByIdAsync(int id)
    {
        var cacheKey = $"category_{id}";
        if (!_cache.TryGetValue(cacheKey, out Category category))
        {
            category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                Log.Warning("Category with id {Id} not found at {Time}", id, DateTime.UtcNow);
                return null;
            }

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            _cache.Set(cacheKey, category, cacheOptions);
            Log.Information("Category with id {Id} retrieved from database and cached at {Time}", id, DateTime.UtcNow);
        }
        else
        {
            Log.Information("Category with id {Id} retrieved from cache at {Time}", id, DateTime.UtcNow);
        }

        return category;
    }

    public async Task AddCategoryAsync(Category category)
    {
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove("categories"); // Invalidate cache
        Log.Information("Category added and cache invalidated at {Time}", DateTime.UtcNow);
    }

    public async Task UpdateCategoryAsync(int id, CategoryDTO categoryDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category != null)
        {
            category.Name = categoryDto.Name;
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"category_{id}"); // Invalidate cache for specific category
            _cache.Remove("categories"); // Invalidate cache for all categories
            Log.Information("Category with id {Id} updated and cache invalidated at {Time}", id, DateTime.UtcNow);
        }
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category != null)
        {
            await _unitOfWork.Categories.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"category_{id}"); // Invalidate cache for specific category
            _cache.Remove("categories"); // Invalidate cache for all categories
            Log.Information("Category with id {Id} deleted and cache invalidated at {Time}", id, DateTime.UtcNow);
        }
    }
}