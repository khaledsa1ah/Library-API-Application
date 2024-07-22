using Day1.DTOs;

namespace Day1.Services;

using Data;

public interface IBookService
{
    Task<IEnumerable<Book>> GetBooksAsync();
    Task<Book> GetBookByISBNAsync(string isbn);
    Task AddBookAsync(BookDTO bookDTO);
    Task UpdateBookAsync(string isbn, BookDTO bookDTO);
    Task DeleteBookAsync(string isbn);
}