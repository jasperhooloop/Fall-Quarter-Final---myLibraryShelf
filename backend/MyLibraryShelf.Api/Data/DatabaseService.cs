using Microsoft.Data.SqlClient;

namespace MyLibraryShelf.Api.Data;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    public SqlConnection CreateConnection() => new(_connectionString);
}
