namespace Day1.Repositories;

using Day1.Data;

public interface IBookRepository : IRepository<Book>
{
    Task<Book> GetBookByISBNAsync(string isbn);
    // Add any other book-specific methods here
}