namespace MiniLibraryMgmtSys.DTO
{
    public class BookDTO
    {
        public class BookDto()
        {
            public string? Id { get; set; } = null!;

            public string? Author { get; set; } = null!;

            public string? Title { get; set; } = null!;

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

            public String Message { get; set; }

            public BookDto? Data { get; set; }
        }

    }
}
