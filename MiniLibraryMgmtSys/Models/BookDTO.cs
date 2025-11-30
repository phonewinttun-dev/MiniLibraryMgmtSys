namespace MiniLibraryMgmtSys.DTO
{
    public class BookDto
    {
        public string? Id { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool DeleteFlag { get; set; }
    }

    public class BookResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = null!;
        public BookDto? Data { get; set; }
    }
}
