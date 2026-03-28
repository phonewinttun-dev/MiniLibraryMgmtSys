using System;
using System.Collections.Generic;

namespace MiniLibraryMgmtSys.Database.AppDbContextModels;

public partial class TblUser
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual ICollection<TblBook> TblBookCreatedByNavigations { get; set; } = new List<TblBook>();

    public virtual ICollection<TblBook> TblBookUpdatedByNavigations { get; set; } = new List<TblBook>();

    public virtual ICollection<TblBorrowedBook> TblBorrowedBooks { get; set; } = new List<TblBorrowedBook>();
}
