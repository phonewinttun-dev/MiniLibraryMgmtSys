using System;
using System.Collections.Generic;

namespace MiniLibraryMgmtSys.Database.AppDbContextModels;

public partial class TblBook
{
    public Guid Id { get; set; }

    public string Author { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Genre { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<TblBorrowedBook> TblBorrowedBooks { get; set; } = new List<TblBorrowedBook>();
}
