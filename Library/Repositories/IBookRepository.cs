namespace Library.Repositories;

using Library.Data;

public interface IBookRepository : IRepository<Book>
{
    Task<Book> GetBookByIsbnAsync(string isbn);
    // Add any other book-specific methods here
    
    // Get all books 
    Task<List<Book>> GetBooksAsync();
    Task<List<Book>> GetBooksByAuthorAsync(int authorId);
    
    Task<List<Book>> GetBooksByCategoryAsync(int categoryId);
    
    
}