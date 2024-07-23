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
        IBookService _bookService) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            var books = await _bookService.GetBooksAsync();
            return Ok(books);
        }

        [HttpGet("{isbn}")]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<Book>> GetBookByISBN(string isbn)
        {
            var book = await _bookService.GetBookByISBNAsync(isbn);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Book>> AddBook(BookDTO bookDTO)
        {
            await _bookService.AddBookAsync(bookDTO);
            return CreatedAtAction(nameof(GetBookByISBN), new { isbn = bookDTO.ISBN }, bookDTO);
        }

        [HttpPut("{isbn}")]
        [CheckPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateBook(string isbn, BookDTO bookDTO)
        {
            await _bookService.UpdateBookAsync(isbn, bookDTO);
            return NoContent();
        }

        [HttpDelete("{isbn}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteBook(string isbn)
        {
            await _bookService.DeleteBookAsync(isbn);
            return NoContent();
        }
    }
}