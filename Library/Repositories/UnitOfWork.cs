namespace Library.Repositories;

using Data;

public class UnitOfWork(
    ApplicationDbContext context,
    IAuthorRepository authors,
    IBookRepository books,
    ICategoryRepository categories) : IUnitOfWork
{
    public IAuthorRepository Authors => authors ??= new AuthorRepository(context);
    public IBookRepository Books => books ??= new BookRepository(context);
    public ICategoryRepository Categories => categories ??= new CategoryRepository(context);

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}