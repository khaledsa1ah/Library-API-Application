namespace Day1.Services;

using Data;
using DTOs;

public interface IAuthorService
{
    Task<IEnumerable<Author>> GetAuthorsAsync();
    Task<Author> GetAuthorByIdAsync(int id);
    Task<Author> AddAuthorAsync(Author author);
    Task UpdateAuthorAsync(int id, AuthorDTO authorDto);
    Task DeleteAuthorAsync(int id);
}