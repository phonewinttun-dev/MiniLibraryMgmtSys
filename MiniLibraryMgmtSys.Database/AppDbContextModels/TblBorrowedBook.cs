using System;
using System.Collections.Generic;

namespace MiniLibraryMgmtSys.Database.AppDbContextModels;

public partial class TblBorrowedBook
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string BookId { get; set; } = null!;

    public DateTime BorrowedAt { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TblBook Book { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
