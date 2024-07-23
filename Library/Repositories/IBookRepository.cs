namespace Library.Repositories;

using Library.Data;

public interface IBookRepository : IRepository<Book>
{
    Task<Book> GetBookByISBNAsync(string isbn);
    // Add any other book-specific methods here
}