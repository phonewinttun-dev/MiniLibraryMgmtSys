namespace MiniLibraryMgmtSys.Infrastructure
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Librarian = "Librarian";
        public const string Member = "Member";

        public static readonly string[] All = { Admin, Librarian, Member };
    }
}
