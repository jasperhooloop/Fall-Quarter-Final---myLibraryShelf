using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyLibraryShelf.Api.Data;
using MyLibraryShelf.Api.Models.Requests;
using MyLibraryShelf.Api.Models.Responses;

namespace MyLibraryShelf.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class BooksController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public BooksController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost]
    public async Task<ActionResult<BookCreatedResponse>> AddBook([FromBody] BookCreateRequest request)
    {
        await using var connection = _databaseService.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_AddBook", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ShelfId", SqlDbType.Int) { Value = request.ShelfId });
        command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = request.Title });
        command.Parameters.Add(new SqlParameter("@Author", SqlDbType.NVarChar, 200) { Value = request.Author });
        command.Parameters.Add(new SqlParameter("@Isbn", SqlDbType.NVarChar, 32)
        {
            Value = string.IsNullOrWhiteSpace(request.Isbn) ? DBNull.Value : request.Isbn
        });
        command.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar, 20) { Value = request.Type });
        command.Parameters.Add(new SqlParameter("@DurationMinutes", SqlDbType.Int)
        {
            Value = request.DurationMinutes.HasValue ? request.DurationMinutes.Value : DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@Narrator", SqlDbType.NVarChar, 200)
        {
            Value = string.IsNullOrWhiteSpace(request.Narrator) ? DBNull.Value : request.Narrator
        });
        command.Parameters.Add(new SqlParameter("@PublishedYear", SqlDbType.Int)
        {
            Value = request.PublishedYear.HasValue ? request.PublishedYear.Value : DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, -1)
        {
            Value = string.IsNullOrWhiteSpace(request.Notes) ? DBNull.Value : request.Notes
        });

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to add book");
        }

        return Ok(new BookCreatedResponse { BookId = Convert.ToInt32(result) });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookListItem>>> GetBooksForShelf([FromQuery] int shelfId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
    {
        await using var connection = _databaseService.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_GetBooksForShelf", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ShelfId", SqlDbType.Int) { Value = shelfId });
        command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

        var items = new List<BookListItem>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new BookListItem
            {
                BookId = reader.GetInt32(0),
                ShelfId = reader.GetInt32(1),
                Title = reader.GetString(2),
                Author = reader.GetString(3),
                Isbn = reader.IsDBNull(4) ? null : reader.GetString(4),
                Type = reader.GetString(5),
                PublishedYear = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                CreatedAt = reader.GetDateTime(7),
                TotalBooksOnShelf = reader.GetInt32(8)
            });
        }

        return Ok(items);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AudiobookListItem>>> GetAudiobooksForShelf([FromQuery] int shelfId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
    {
        await using var connection = _databaseService.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_GetAudiobooksForShelf", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ShelfId", SqlDbType.Int) { Value = shelfId });
        command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

        var items = new List<AudiobookListItem>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new AudiobookListItem
            {
                BookId = reader.GetInt32(0),
                ShelfId = reader.GetInt32(1),
                Title = reader.GetString(2),
                Author = reader.GetString(3),
                Isbn = reader.IsDBNull(4) ? null : reader.GetString(4),
                Type = reader.GetString(5),
                DurationMinutes = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                Narrator = reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt = reader.GetDateTime(8),
                TotalAudiobooksOnShelf = reader.GetInt32(9)
            });
        }

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveBook([FromBody] BookRemoveRequest request)
    {
        if (request.BookId <= 0 || request.ShelfId <= 0)
        {
            return BadRequest("Invalid shelf or book identifier.");
        }

        await using var connection = _databaseService.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_RemoveBook", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ShelfId", SqlDbType.Int) { Value = request.ShelfId });
        command.Parameters.Add(new SqlParameter("@BookId", SqlDbType.Int) { Value = request.BookId });

        var result = await command.ExecuteScalarAsync();
        var rowsAffected = result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);

        if (rowsAffected == 0)
        {
            return NotFound();
        }

        return NoContent();
    }
}
