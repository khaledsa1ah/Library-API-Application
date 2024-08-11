namespace Library.Services;
using Data;
using DTOs;
public interface ICategoryService
{
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<Category> GetCategoryByIdAsync(int id);
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(int id, CategoryDto categoryDto);
    Task DeleteCategoryAsync(int id);
}