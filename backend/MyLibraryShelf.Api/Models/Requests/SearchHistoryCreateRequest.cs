namespace MyLibraryShelf.Api.Models.Requests;

public sealed class SearchHistoryCreateRequest
{
    public int ShelfId { get; set; }
    public string QueryText { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
}
