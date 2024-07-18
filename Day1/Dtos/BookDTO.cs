namespace MyWebAPP.DTOs;

public class BookDTO
{
    public string Title { get; set; }
    public string ISBN { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
}