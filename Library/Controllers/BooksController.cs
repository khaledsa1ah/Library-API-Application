using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Authorization;
using Library.DTOs;
using Serilog;
using Microsoft.Extensions.Caching.Memory;
using Library.Repositories;
using Library.Services;

namespace Library.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class BooksController(
        IBookService bookService) : ControllerBase
    {
[HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<GetBooksDto>>> GetBooks()
        {
            Log.Information("Getting list of books.");
            var books = await bookService.GetBooksAsync();
            Log.Information("Retrieved {Count} books.", books.Count());
            return Ok(books);
        }

        [HttpGet("{isbn}")]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<Book>> GetBookByIsbn(string isbn)
        {
            Log.Information("Getting book with ISBN {ISBN}.", isbn);
            var book = await bookService.GetBookByIsbnAsync(isbn);
            if (book == null)
            {
                Log.Warning("Book with ISBN {ISBN} not found.", isbn);
                return NotFound();
            }
            Log.Information("Retrieved book with ISBN {ISBN}.", isbn);
            return Ok(book);
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Book>> AddBook(BookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Log.Information("Adding new book with ISBN {ISBN}.", bookDto.Isbn);
            await bookService.AddBookAsync(bookDto);
            Log.Information("Book with ISBN {ISBN} added.", bookDto.Isbn);
            return CreatedAtAction(nameof(GetBookByIsbn), new { isbn = bookDto.Isbn }, bookDto);
        }

        [HttpPut("{isbn}")]
        [CheckPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateBook(string isbn, BookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Log.Information("Updating book with ISBN {ISBN}.", isbn);
            await bookService.UpdateBookAsync(isbn, bookDto);
            Log.Information("Book with ISBN {ISBN} updated.", isbn);
            return NoContent();
        }

        [HttpDelete("{isbn}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteBook(string isbn)
        {
            Log.Information("Deleting book with ISBN {ISBN}.", isbn);
            await bookService.DeleteBookAsync(isbn);
            Log.Information("Book with ISBN {ISBN} deleted.", isbn);
            return NoContent();
        }
    }
}