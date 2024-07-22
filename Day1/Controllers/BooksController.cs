using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Day1.Data;
using MyWebAPP.DTOs;
using Day1.Authorization;
using Serilog;
using Microsoft.Extensions.Caching.Memory;
using Day1.Repositories;

namespace MyWebAPP.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class BooksController(
        ApplicationDbContext _context,
        IMemoryCache _cache,
        BookRepository _bookRepository,
        CategoryRepository _categoryRepository,
        AuthorRepository _authorRepository) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            Log.Information("GetBooks called at {Time}", DateTime.UtcNow);

            var cacheKey = "books";
            if (!_cache.TryGetValue(cacheKey, out List<Book> books))
            {
                books = (await _bookRepository.GetAllAsync()).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, books, cacheOptions);
                Log.Information("Books retrieved from database and cached at {Time}", DateTime.UtcNow);
            }
            else
            {
                Log.Information("Books retrieved from cache at {Time}", DateTime.UtcNow);
            }

            return books;
        }

        [HttpGet("{isbn}")]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<Book>> GetBookByISBN(string isbn)
        {
            Log.Information("GetBookByISBN called with ISBN {ISBN} at {Time}", isbn, DateTime.UtcNow);

            var cacheKey = $"book_{isbn}";
            if (!_cache.TryGetValue(cacheKey, out Book book))
            {
                book = await _bookRepository.GetBookByISBNAsync(isbn);

                if (book == null)
                {
                    Log.Warning("Book with ISBN {ISBN} not found at {Time}", isbn, DateTime.UtcNow);
                    return NotFound();
                }

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, book, cacheOptions);
                Log.Information("Book with ISBN {ISBN} retrieved from database and cached at {Time}", isbn,
                    DateTime.UtcNow);
            }
            else
            {
                Log.Information("Book with ISBN {ISBN} retrieved from cache at {Time}", isbn, DateTime.UtcNow);
            }

            return book;
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Book>> AddBook(BookDTO bookDTO)
        {
            Log.Information("AddBook called at {Time}", DateTime.UtcNow);

            var author = await _authorRepository.GetByIdAsync(bookDTO.AuthorId);
            if (author == null)
            {
                return BadRequest("Invalid author ID.");
            }

            var category = await _categoryRepository.GetByIdAsync(bookDTO.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid category ID.");
            }

            var book = new Book
            {
                Title = bookDTO.Title,
                ISBN = bookDTO.ISBN,
                AuthorId = bookDTO.AuthorId,
                CategoryId = bookDTO.CategoryId,
                Category = category,
                Author = author
            };

            await _bookRepository.AddAsync(book);

            var cacheKey = "books";
            _cache.Remove(cacheKey); // Invalidate cache

            Log.Information("Book added and cache invalidated at {Time}", DateTime.UtcNow);

            return CreatedAtAction(nameof(GetBookByISBN), new { isbn = book.ISBN }, book);
        }

        [HttpPut("{isbn}")]
        [CheckPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateBook(string isbn, BookDTO bookDTO)
        {
            var book = await _bookRepository.GetBookByISBNAsync(isbn);

            if (book == null)
            {
                return NotFound();
            }

            Log.Information("UpdateBook called for ISBN {ISBN} at {Time}", isbn, DateTime.UtcNow);

            var author = await _authorRepository.GetByIdAsync(bookDTO.AuthorId);
            if (author == null)
            {
                return BadRequest("Invalid author ID.");
            }

            var category = await _categoryRepository.GetByIdAsync(bookDTO.CategoryId);
            if (category == null)
            {
                return BadRequest("Invalid category ID.");
            }

            book.Title = bookDTO.Title;
            book.ISBN = bookDTO.ISBN;
            book.AuthorId = bookDTO.AuthorId;
            book.CategoryId = bookDTO.CategoryId;
            book.Category = category;
            book.Author = author;

            await _bookRepository.UpdateAsync(book);

            var cacheKey = $"book_{isbn}";
            _cache.Remove(cacheKey); // Invalidate cache for specific book
            _cache.Remove("books"); // Invalidate cache for all books

            Log.Information("Book with ISBN {ISBN} updated and cache invalidated at {Time}", isbn,
                DateTime.UtcNow);

            return NoContent();
        }

        [HttpDelete("{isbn}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteBook(string isbn)
        {
            Log.Information("DeleteBook called for ISBN {ISBN} at {Time}", isbn, DateTime.UtcNow);

            var book = await _bookRepository.GetBookByISBNAsync(isbn);
            if (book == null)
            {
                return NotFound();
            }

            await _bookRepository.DeleteAsync(book);

            var cacheKey = $"book_{isbn}";
            _cache.Remove(cacheKey); // Invalidate cache for specific book
            _cache.Remove("books"); // Invalidate cache for all books

            Log.Information("Book with ISBN {ISBN} deleted and cache invalidated at {Time}", isbn,
                DateTime.UtcNow);

            return NoContent();
        }
    }
}