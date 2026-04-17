using System;
using System.Collections.Generic;

namespace MiniLibraryMgmtSys.DTO
{
    public class BorrowRequestDto
    {
        public string BookId { get; set; } = string.Empty;
    }

    public class BorrowResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string BookId { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public bool IsOverdue { get; set; }
    }
}
