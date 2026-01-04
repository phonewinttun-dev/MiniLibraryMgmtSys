using System;
using System.Collections.Generic;

namespace MiniLibraryMgmtSys.Database.AppDbContextModels;

public partial class TblBook
{
    public string Id { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Genre { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblUser? CreatedByNavigation { get; set; }

    public virtual ICollection<TblBorrowedBook> TblBorrowedBooks { get; set; } = new List<TblBorrowedBook>();

    public virtual TblUser? UpdatedByNavigation { get; set; }
}
