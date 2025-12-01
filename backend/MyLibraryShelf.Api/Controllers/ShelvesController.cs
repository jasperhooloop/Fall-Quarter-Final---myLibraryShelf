using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MyLibraryShelf.Api.Data;
using MyLibraryShelf.Api.Models.Requests;
using MyLibraryShelf.Api.Models.Responses;

namespace MyLibraryShelf.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ShelvesController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public ShelvesController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost]
    public async Task<ActionResult<ShelfCreatedResponse>> CreateShelf([FromBody] ShelfCreateRequest request)
    {
        await using var connection = _databaseService.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_CreateShelf", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ShelfName", SqlDbType.NVarChar, 100) { Value = request.ShelfName });
        command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 512) { Value = request.PasswordHash });

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to create shelf");
        }

        var response = new ShelfCreatedResponse { ShelfId = Convert.ToInt32(result) };
        return CreatedAtAction(nameof(CreateShelf), response);
    }

    [HttpPost]
    public async Task<ActionResult<ShelfCreatedResponse>> AuthenticateShelf([FromBody] ShelfAuthenticateRequest request)
    {
        await using var connection = _databaseService.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.sp_AuthenticateShelf", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ShelfName", SqlDbType.NVarChar, 100) { Value = request.ShelfName });
        command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 512) { Value = request.PasswordHash });

        var result = await command.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            return Unauthorized();
        }

        return Ok(new ShelfCreatedResponse { ShelfId = Convert.ToInt32(result) });
    }
}
