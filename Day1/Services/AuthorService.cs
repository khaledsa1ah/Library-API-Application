using Day1.Data;
using Day1.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Day1.Services;

public class AuthorService(UnitOfWork _unitOfWork, IMemoryCache _cache) : IAuthorService
{
    public async Task<IEnumerable<Author>> GetAuthorsAsync()
    {
        var cacheKey = "authors";
        if (!_cache.TryGetValue(cacheKey, out List<Author> authors))
        {
            authors = (await _unitOfWork.Authors.GetAllAsync()).ToList();
            _cache.Set(cacheKey, authors, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        return authors;
    }

    public async Task<Author> GetAuthorByIdAsync(int id)
    {
        var cacheKey = $"author_{id}";
        if (!_cache.TryGetValue(cacheKey, out Author author))
        {
            author = await _unitOfWork.Authors.GetByIdAsync(id);
            if (author == null) return null;
            _cache.Set(cacheKey, author, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        return author;
    }

    public async Task<Author> AddAuthorAsync(Author author)
    {
        await _unitOfWork.Authors.AddAsync(author);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove("authors"); // Invalidate cache
        return author;
    }

    public async Task UpdateAuthorAsync(Author author)
    {
        await _unitOfWork.Authors.UpdateAsync(author);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove($"author_{author.Id}"); // Invalidate cache for specific author
        _cache.Remove("authors"); // Invalidate cache for all authors
    }

    public async Task DeleteAuthorAsync(int id)
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(id);
        if (author != null)
        {
            await _unitOfWork.Authors.DeleteAsync(author);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"author_{id}"); // Invalidate cache for specific author
            _cache.Remove("authors"); // Invalidate cache for all authors
        }
    }
}

