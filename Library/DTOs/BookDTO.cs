namespace Library.DTOs;
using System.ComponentModel.DataAnnotations;
public class BookDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    public string Title { get; set; }

    [Required(ErrorMessage = "ISBN is required")]
    [RegularExpression(@"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$", 
        ErrorMessage = "ISBN format is not valid")]
    public string Isbn { get; set; }

    [Required(ErrorMessage = "Author ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Author ID must be a positive integer")]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Category ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive integer")]
    public int CategoryId { get; set; }
}