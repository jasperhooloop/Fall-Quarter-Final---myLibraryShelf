namespace MyLibraryShelf.Api.Models.Requests;

public sealed class BookRemoveRequest
{
    public int ShelfId { get; set; }
    public int BookId { get; set; }
}
