namespace MyLibraryShelf.Api.Models.Requests;

public sealed class ShelfAuthenticateRequest
{
    public string ShelfName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
