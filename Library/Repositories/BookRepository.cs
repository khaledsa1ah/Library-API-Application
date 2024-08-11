namespace Library.Repositories;

using Library.Data;
using Microsoft.EntityFrameworkCore;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Book> GetBookByIsbnAsync(string isbn)
    {
        // return the book with the specified ISBN and include the author and category
        return await Context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Isbn == isbn) ?? throw new InvalidOperationException();
    }

    public Task<List<Book>> GetBooksAsync()
    {
        // return all books and include the author and category
        return Context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .ToListAsync();
    }

    public Task<List<Book>> GetBooksByAuthorAsync(int authorId)
    {
        // return all books by a specific author
        return Context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.AuthorId == authorId)
            .ToListAsync();
    }

    public Task<List<Book>> GetBooksByCategoryAsync(int categoryId)
    {
        // return all books in a specific category
        return Context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Where(b => b.CategoryId == categoryId)
            .ToListAsync();
    }
}