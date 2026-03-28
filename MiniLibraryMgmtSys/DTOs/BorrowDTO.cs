using System;
using System.Collections.Generic;

namespace MiniLibraryMgmtSys.DTO
{
    public class BorrowRequestDto
    {
        public string BookId { get; set; } = null!;
    }

    public class BorrowResponseDto
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string BookId { get; set; } = null!;
        public string BookTitle { get; set; } = null!;
        public string BookAuthor { get; set; } = null!;
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public bool IsOverdue { get; set; }
    }
}
