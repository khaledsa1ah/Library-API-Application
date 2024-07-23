namespace Library.Repositories;

using Library.Data;
using Microsoft.EntityFrameworkCore;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Book> GetBookByISBNAsync(string isbn)
    {
        return await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.ISBN == isbn) ?? throw new InvalidOperationException();
    }
}