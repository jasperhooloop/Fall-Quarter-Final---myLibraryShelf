namespace MyLibraryShelf.Api.Models.Requests;

public sealed class BookCreateRequest
{
    public int ShelfId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public string? Narrator { get; set; }
    public int? PublishedYear { get; set; }
    public string? Notes { get; set; }
}
