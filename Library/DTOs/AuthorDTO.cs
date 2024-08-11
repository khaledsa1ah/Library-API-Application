namespace Library.DTOs;
using System.ComponentModel.DataAnnotations;
public class AuthorDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; }
    
    [StringLength(1000, ErrorMessage = "Biography cannot be longer than 1000 characters")]
    public string Biography { get; set; }
}