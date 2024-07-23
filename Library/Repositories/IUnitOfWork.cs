namespace Library.Repositories;

public interface IUnitOfWork : IDisposable
{
    IAuthorRepository Authors { get; }
    IBookRepository Books { get; }
    ICategoryRepository Categories { get; }
    Task<int> SaveChangesAsync();
}