using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Day1.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Day1.Authorization;
using Day1.Repositories;
using Serilog;

namespace MyWebAPP.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class AuthorsController(ApplicationDbContext _context, IMemoryCache _cache, AuthorRepository _authorRepository) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            Log.Information("GetAuthors called at {Time}", DateTime.UtcNow);
            
            var cacheKey = "authors";
            if (!_cache.TryGetValue(cacheKey, out List<Author> authors))
            {
                authors = (await _authorRepository.GetAllAsync()).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, authors, cacheOptions);
                Log.Information("Authors retrieved from database and cached at {Time}", DateTime.UtcNow);
            }
            else
            {
                Log.Information("Authors retrieved from cache at {Time}", DateTime.UtcNow);
            }

            return authors;
        }

        [CheckPermission(Permission.Read)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Author>> GetAuthorById(int id)
        {
            Log.Information("GetAuthorById called with id {Id} at {Time}", id, DateTime.UtcNow);

            var cacheKey = $"author_{id}";
            if (!_cache.TryGetValue(cacheKey, out Author author))
            {
                author = await _authorRepository.GetByIdAsync(id);

                if (author == null)
                {
                    Log.Warning("Author with id {Id} not found at {Time}", id, DateTime.UtcNow);
                    return NotFound();
                }

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, author, cacheOptions);
                Log.Information("Author with id {Id} retrieved from database and cached at {Time}", id,
                    DateTime.UtcNow);
            }
            else
            {
                Log.Information("Author with id {Id} retrieved from cache at {Time}", id, DateTime.UtcNow);
            }

            return author;
        }

        [CheckPermission(Permission.Add)]
        [HttpPost]
        public async Task<ActionResult<Author>> AddAuthor(Author author)
        {
            Log.Information("AddAuthor called at {Time}", DateTime.UtcNow);

            await _authorRepository.AddAsync(author);

            var cacheKey = "authors";
            _cache.Remove(cacheKey); // Invalidate cache

            Log.Information("Author added and cache invalidated at {Time}", DateTime.UtcNow);

            return CreatedAtAction(nameof(GetAuthorById), new { id = author.Id }, author);
        }

        [CheckPermission(Permission.Edit)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, Author author)
        {
            if (id != author.Id)
            {
                return BadRequest();
            }

            Log.Information("UpdateAuthor called for id {Id} at {Time}", id, DateTime.UtcNow);

            try
            {
                await _authorRepository.UpdateAsync(author);

                var cacheKey = $"author_{id}";
                _cache.Remove(cacheKey); // Invalidate cache for specific author
                _cache.Remove("authors"); // Invalidate cache for all authors

                Log.Information("Author with id {Id} updated and cache invalidated at {Time}", id,
                    DateTime.UtcNow);
            }
            catch (Exception)
            {
                if (!await AuthorExists(id))
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

        [CheckPermission(Permission.Delete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            Log.Information("DeleteAuthor called for id {Id} at {Time}", id, DateTime.UtcNow);

            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            await _authorRepository.DeleteAsync(author);

            var cacheKey = $"author_{id}";
            _cache.Remove(cacheKey); // Invalidate cache for specific author
            _cache.Remove("authors"); // Invalidate cache for all authors

            Log.Information("Author with id {Id} deleted and cache invalidated at {Time}", id, DateTime.UtcNow);

            return NoContent();
        }

        private async Task<bool> AuthorExists(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            return author != null;
        }
    }
}