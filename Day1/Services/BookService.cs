namespace Day1.Services;

using Repositories;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Data;
using DTOs;

public class BookService(UnitOfWork _unitOfWork, IMemoryCache _cache) : IBookService
{
    public async Task<IEnumerable<Book>> GetBooksAsync()
    {
        var cacheKey = "books";
        if (!_cache.TryGetValue(cacheKey, out List<Book> books))
        {
            books = (await _unitOfWork.Books.GetAllAsync()).ToList();
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

    public async Task<Book> GetBookByISBNAsync(string isbn)
    {
        var cacheKey = $"book_{isbn}";
        if (!_cache.TryGetValue(cacheKey, out Book book))
        {
            book = await _unitOfWork.Books.GetBookByISBNAsync(isbn);
            if (book == null)
            {
                Log.Warning("Book with ISBN {ISBN} not found at {Time}", isbn, DateTime.UtcNow);
                return null;
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

    public async Task AddBookAsync(BookDTO bookDTO)
    {
        var author = await _unitOfWork.Authors.GetByIdAsync(bookDTO.AuthorId);
        if (author == null)
        {
            throw new ArgumentException("Invalid author ID.");
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(bookDTO.CategoryId);
        if (category == null)
        {
            throw new ArgumentException("Invalid category ID.");
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
        await _unitOfWork.Books.AddAsync(book);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove("books"); // Invalidate cache
        Log.Information("Book added and cache invalidated at {Time}", DateTime.UtcNow);
    }

    public async Task UpdateBookAsync(string isbn, BookDTO bookDTO)
    {
        var book = await _unitOfWork.Books.GetBookByISBNAsync(isbn);
        if (book == null)
        {
            throw new KeyNotFoundException("Book not found.");
        }

        book.Title = bookDTO.Title;
        book.AuthorId = bookDTO.AuthorId;
        book.CategoryId = bookDTO.CategoryId;
        await _unitOfWork.Books.UpdateAsync(book);
        await _unitOfWork.SaveChangesAsync();
        _cache.Remove($"book_{isbn}"); // Invalidate cache for specific book
        _cache.Remove("books"); // Invalidate cache for all books
        Log.Information("Book with ISBN {ISBN} updated and cache invalidated at {Time}", isbn, DateTime.UtcNow);
    }

    public async Task DeleteBookAsync(string isbn)
    {
        var book = await _unitOfWork.Books.GetBookByISBNAsync(isbn);
        if (book != null)
        {
            await _unitOfWork.Books.DeleteAsync(book);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove($"book_{isbn}"); // Invalidate cache for specific book
            _cache.Remove("books"); // Invalidate cache for all books
            Log.Information("Book with ISBN {ISBN} deleted and cache invalidated at {Time}", isbn, DateTime.UtcNow);
        }
    }
}