using System;
using System.Collections.Generic;

namespace MiniLibraryMgmtSys.Database.AppDbContextModels;

public partial class TblUser
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblUser? CreatedByNavigation { get; set; }

    public virtual ICollection<TblUser> InverseCreatedByNavigation { get; set; } = new List<TblUser>();

    public virtual ICollection<TblUser> InverseUpdatedByNavigation { get; set; } = new List<TblUser>();

    public virtual ICollection<TblBook> TblBookCreatedByNavigations { get; set; } = new List<TblBook>();

    public virtual ICollection<TblBook> TblBookUpdatedByNavigations { get; set; } = new List<TblBook>();

    public virtual ICollection<TblBorrowedBook> TblBorrowedBooks { get; set; } = new List<TblBorrowedBook>();

    public virtual TblUser? UpdatedByNavigation { get; set; }
}
