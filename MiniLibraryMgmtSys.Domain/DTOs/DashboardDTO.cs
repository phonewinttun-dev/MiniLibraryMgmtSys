namespace MiniLibraryMgmtSys.Domain.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalBooks { get; set; }
        public int AvailableBooksCount { get; set; }
        public int BorrowedBooksCount { get; set; }
        public int TotalRegisteredUsersCount { get; set; }
        public int ActiveBorrowCount { get; set; }
        public int OverdueBorrowCount { get; set; }
    }
}
