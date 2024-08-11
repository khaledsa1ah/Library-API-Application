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
    public class AuthorsController(IAuthorService authorService) : ControllerBase
    {
        [HttpGet]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            Log.Information("Getting list of authors.");
            var authors = await authorService.GetAuthorsAsync();
            Log.Information("Retrieved {Count} authors.", authors.Count());
            return Ok(authors);
        }

        [HttpGet("{id}")]
        [CheckPermission(Permission.Read)]
        public async Task<ActionResult<Author>> GetAuthorById(int id)
        {
            Log.Information("Getting author with ID {Id}.", id);
            var author = await authorService.GetAuthorByIdAsync(id);
            if (author == null)
            {
                Log.Warning("Author with ID {Id} not found.", id);
                return NotFound();
            }

            Log.Information("Retrieved author with ID {Id}.", id);
            return Ok(author);
        }

        [HttpPost]
        [CheckPermission(Permission.Add)]
        public async Task<ActionResult<Author>> AddAuthor(Author author)
        {
            Log.Information("Adding new author with Name {Name}.", author.Name);
            await authorService.AddAuthorAsync(author);
            Log.Information("Author with ID {Id} added.", author.Id);
            return CreatedAtAction(nameof(GetAuthorById), new { id = author.Id }, author);
        }

        [HttpPut]
        [CheckPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateAuthor([FromBody] AuthorDto authorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Log.Information("Updating author with ID {Id}.", authorDto.Id);
            await authorService.UpdateAuthorAsync(authorDto);
            Log.Information("Author with ID {Id} updated.", authorDto.Id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [CheckPermission(Permission.Delete)]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            Log.Information("Deleting author with ID {Id}.", id);
            await authorService.DeleteAuthorAsync(id);
            Log.Information("Author with ID {Id} deleted.", id);
            return NoContent();
        }
    }
}