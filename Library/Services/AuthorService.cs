using Library.Data;
using Library.DTOs;
using Library.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Library.Services;

public class AuthorService(UnitOfWork unitOfWork, IMemoryCache cache) : IAuthorService
{
    public async Task<IEnumerable<Author>> GetAuthorsAsync()
    {
        var cacheKey = "authors";
        if (!cache.TryGetValue(cacheKey, out List<Author> authors))
        {
            Log.Information("Cache miss for authors. Fetching from database.");
            authors = (await unitOfWork.Authors.GetAllAsync()).ToList();
            cache.Set(cacheKey, authors, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            Log.Information("Fetched {Count} authors and updated cache.", authors.Count);
        }
        else
        {
            Log.Information("Cache hit for authors.");
        }

        return authors;
    }

    public async Task<Author> GetAuthorByIdAsync(int id)
    {
        var cacheKey = $"author_{id}";
        if (!cache.TryGetValue(cacheKey, out Author author))
        {
            Log.Information("Cache miss for author with ID {Id}. Fetching from database.", id);
            author = await unitOfWork.Authors.GetByIdAsync(id);
            if (author == null)
            {
                Log.Warning("Author with ID {Id} not found.", id);
                return null;
            }

            cache.Set(cacheKey, author, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            Log.Information("Fetched author with ID {Id} and updated cache.", id);
        }
        else
        {
            Log.Information("Cache hit for author with ID {Id}.", id);
        }

        return author;
    }

    public async Task<Author> AddAuthorAsync(Author author)
    {
        Log.Information("Adding new author with Name {Name}.", author.Name);
        await unitOfWork.Authors.AddAsync(author);
        await unitOfWork.SaveChangesAsync();
        cache.Remove("authors"); // Invalidate cache
        Log.Information("Author with ID {Id} added and cache invalidated.", author.Id);
        return author;
    }

    public async Task UpdateAuthorAsync(AuthorDto authorDto)
    {
        Log.Information("Updating author with ID {Id}.", authorDto.Id);
        var author = await unitOfWork.Authors.GetByIdAsync(authorDto.Id);
        if (author != null)
        {
            author.Name = authorDto.Name;
            author.Biography = authorDto.Biography;
            await unitOfWork.SaveChangesAsync();
            cache.Remove($"author_{authorDto.Id}"); // Invalidate cache for specific author
            cache.Remove("authors"); // Invalidate cache for all authors
            Log.Information("Author with ID {Id} updated and caches invalidated.", authorDto.Id);
        }
        else
        {
            Log.Warning("Author with ID {Id} not found for update.", authorDto.Id);
        }
    }

    public async Task DeleteAuthorAsync(int id)
    {
        Log.Information("Deleting author with ID {Id}.", id);
        var author = await unitOfWork.Authors.GetByIdAsync(id);
        if (author != null)
        {
            await unitOfWork.Authors.DeleteAsync(author);
            await unitOfWork.SaveChangesAsync();
            cache.Remove($"author_{id}"); // Invalidate cache for specific author
            cache.Remove("authors"); // Invalidate cache for all authors
            Log.Information("Author with ID {Id} deleted and caches invalidated.", id);
        }
        else
        {
            Log.Warning("Author with ID {Id} not found for deletion.", id);
        }
    }
}