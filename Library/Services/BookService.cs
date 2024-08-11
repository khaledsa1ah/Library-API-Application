namespace Library.Services;

using Repositories;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Data;
using DTOs;
using AutoMapper;

public class BookService(IMapper mapper, UnitOfWork unitOfWork, IMemoryCache cache) : IBookService
{
    public async Task<IEnumerable<GetBooksDto>> GetBooksAsync()
    {
        var cacheKey = "books";
        List<Book> books;

        if (!cache.TryGetValue(cacheKey, out books))
        {
            try
            {
                books = (await unitOfWork.Books.GetBooksAsync()).ToList();
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                cache.Set(cacheKey, books, cacheOptions);
                Log.Information("Books retrieved from database and cached at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve books from the database at {Time}", DateTime.UtcNow);
                throw; // Re-throw to let global exception handler manage it
            }
        }
        else
        {
            Log.Information("Books retrieved from cache at {Time}", DateTime.UtcNow);
        }

        return mapper.Map<List<Book>, List<GetBooksDto>>(books);
    }

    public async Task<Book> GetBookByIsbnAsync(string isbn)
    {
        var cacheKey = $"book_{isbn}";
        Book book;

        if (!cache.TryGetValue(cacheKey, out book))
        {
            try
            {
                book = await unitOfWork.Books.GetBookByIsbnAsync(isbn);
                if (book == null)
                {
                    Log.Warning("Book with ISBN {ISBN} not found at {Time}", isbn, DateTime.UtcNow);
                    return null;
                }

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                cache.Set(cacheKey, book, cacheOptions);
                Log.Information("Book with ISBN {ISBN} retrieved from database and cached at {Time}", isbn,
                    DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve book with ISBN {ISBN} from the database at {Time}", isbn,
                    DateTime.UtcNow);
                throw; // Re-throw to let global exception handler manage it
            }
        }
        else
        {
            Log.Information("Book with ISBN {ISBN} retrieved from cache at {Time}", isbn, DateTime.UtcNow);
        }

        return book;
    }

    public async Task AddBookAsync(BookDto bookDto)
    {
        try
        {
            var author = await unitOfWork.Authors.GetByIdAsync(bookDto.AuthorId);
            if (author == null)
            {
                Log.Error("Invalid author ID {AuthorId} provided for book with ISBN {ISBN} at {Time}", bookDto.AuthorId,
                    bookDto.Isbn, DateTime.UtcNow);
                throw new ArgumentException("Invalid author ID.");
            }

            var category = await unitOfWork.Categories.GetByIdAsync(bookDto.CategoryId);
            if (category == null)
            {
                Log.Error("Invalid category ID {CategoryId} provided for book with ISBN {ISBN} at {Time}",
                    bookDto.CategoryId, bookDto.Isbn, DateTime.UtcNow);
                throw new ArgumentException("Invalid category ID.");
            }

            var book = new Book
            {
                Title = bookDto.Title,
                Isbn = bookDto.Isbn,
                AuthorId = bookDto.AuthorId,
                CategoryId = bookDto.CategoryId,
                Category = category,
                Author = author
            };

            await unitOfWork.Books.AddAsync(book);
            await unitOfWork.SaveChangesAsync();
            cache.Remove("books"); // Invalidate cache

            Log.Information("Book with ISBN {ISBN} added and cache invalidated at {Time}", bookDto.Isbn,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add book with ISBN {ISBN} at {Time}", bookDto.Isbn, DateTime.UtcNow);
            throw; // Re-throw to let global exception handler manage it
        }
    }

    public async Task UpdateBookAsync(string isbn, BookDto bookDto)
    {
        try
        {
            var book = await unitOfWork.Books.GetBookByIsbnAsync(isbn);
            if (book == null)
            {
                Log.Warning("Book with ISBN {ISBN} not found for update at {Time}", isbn, DateTime.UtcNow);
                throw new KeyNotFoundException("Book not found.");
            }

            book = mapper.Map(bookDto, book);
            await unitOfWork.Books.UpdateAsync(book);
            await unitOfWork.SaveChangesAsync();
            cache.Remove($"book_{isbn}"); // Invalidate cache for specific book
            cache.Remove("books"); // Invalidate cache for all books

            Log.Information("Book with ISBN {ISBN} updated and cache invalidated at {Time}", isbn, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update book with ISBN {ISBN} at {Time}", isbn, DateTime.UtcNow);
            throw; // Re-throw to let global exception handler manage it
        }
    }

    public async Task DeleteBookAsync(string isbn)
    {
        try
        {
            var book = await unitOfWork.Books.GetBookByIsbnAsync(isbn);
            if (book != null)
            {
                await unitOfWork.Books.DeleteAsync(book);
                await unitOfWork.SaveChangesAsync();
                cache.Remove($"book_{isbn}"); // Invalidate cache for specific book
                cache.Remove("books"); // Invalidate cache for all books

                Log.Information("Book with ISBN {ISBN} deleted and cache invalidated at {Time}", isbn, DateTime.UtcNow);
            }
            else
            {
                Log.Warning("Attempted to delete non-existent book with ISBN {ISBN} at {Time}", isbn, DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete book with ISBN {ISBN} at {Time}", isbn, DateTime.UtcNow);
            throw; // Re-throw to let global exception handler manage it
        }
    }
}