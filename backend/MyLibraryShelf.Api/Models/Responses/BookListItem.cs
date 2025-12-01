namespace MyLibraryShelf.Api.Models.Responses;

public sealed class BookListItem
{
    public int BookId { get; set; }
    public int ShelfId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? PublishedYear { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalBooksOnShelf { get; set; }
}
