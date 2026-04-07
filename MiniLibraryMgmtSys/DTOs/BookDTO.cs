using System.ComponentModel.DataAnnotations;

namespace MiniLibraryMgmtSys.DTO
{
    public class BookDto
    {
        public string? Id { get; set; } = null!;
        [Required]
        public string Author { get; set; } = null!;
        [Required]
        public string Title { get; set; } = null!;
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
        public string Author { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Genre { get; set; } = null!;
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
