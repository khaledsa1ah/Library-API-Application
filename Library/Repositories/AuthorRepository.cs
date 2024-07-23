namespace Library.Repositories;

using Library.Data;

public class AuthorRepository : Repository<Author>, IAuthorRepository
{
    public AuthorRepository(ApplicationDbContext context) : base(context)
    {
    }
}