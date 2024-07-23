using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Library.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using Library.Authorization;
using Library.DTOs;
using Library.Repositories;
using Serilog;
using Library.Services;

namespace Library.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class AuthorsController(IAuthorService _authorService ) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authors = await _authorService.GetAuthorsAsync();
            return Ok(authors);
        }

        [HttpGet("{id}")]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<Author>> GetAuthorById(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null) return NotFound();
            return Ok(author);
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Author>> AddAuthor(Author author)
        {
            await _authorService.AddAuthorAsync(author);
            return CreatedAtAction(nameof(GetAuthorById), new { id = author.Id }, author);
        }

        [HttpPut("{id}")]
        [CheckPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorDTO authorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _authorService.UpdateAuthorAsync(id, authorDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            await _authorService.DeleteAuthorAsync(id);
            return NoContent();
        }
    }
}