using System.ComponentModel.DataAnnotations;

namespace MiniLibraryMgmtSys.Domain.DTOs
{
    public class BookDto
    {
        public string Id { get; set; } = string.Empty;
        [Required]
        public string Author { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public bool IsAvailable { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class CreateBookDto
    {
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
    }

    public class UpdateBookDto
    {
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class SearchBookDto
    {
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
    }

}
