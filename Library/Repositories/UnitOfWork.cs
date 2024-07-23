namespace Library.Repositories;

using Data;

public class UnitOfWork(
    ApplicationDbContext _context,
    IAuthorRepository _authors,
    IBookRepository _books,
    ICategoryRepository _categories) : IUnitOfWork
{
    public IAuthorRepository Authors => _authors ??= new AuthorRepository(_context);
    public IBookRepository Books => _books ??= new BookRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}