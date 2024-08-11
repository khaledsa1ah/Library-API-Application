using Library.DTOs;

namespace Library.Services;

using Data;

public interface IBookService
{
    Task<IEnumerable<GetBooksDto>> GetBooksAsync();
    Task<Book> GetBookByIsbnAsync(string isbn);
    Task AddBookAsync(BookDto bookDto);
    Task UpdateBookAsync(string isbn, BookDto bookDto);
    Task DeleteBookAsync(string isbn);
}