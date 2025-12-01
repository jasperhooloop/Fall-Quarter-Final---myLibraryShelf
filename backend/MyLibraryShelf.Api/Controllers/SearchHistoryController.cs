using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyLibraryShelf.Api.Data;
using MyLibraryShelf.Api.Models.Requests;

namespace MyLibraryShelf.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SearchHistoryController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public SearchHistoryController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost]
    public async Task<IActionResult> AddSearchHistory([FromBody] SearchHistoryCreateRequest request)
    {
        await using var connection = _databaseService.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_AddSearchHistory", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ShelfId", SqlDbType.Int) { Value = request.ShelfId });
        command.Parameters.Add(new SqlParameter("@QueryText", SqlDbType.NVarChar, 400) { Value = request.QueryText });
        command.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.NVarChar, 50) { Value = request.EntityType });
        command.Parameters.Add(new SqlParameter("@EntityId", SqlDbType.Int)
        {
            Value = request.EntityId.HasValue ? request.EntityId.Value : DBNull.Value
        });

        await command.ExecuteNonQueryAsync();
        return NoContent();
    }
}
