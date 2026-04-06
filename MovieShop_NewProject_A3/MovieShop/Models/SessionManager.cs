namespace MovieShop.Models
{
    public static class SessionManager
    {
        public const int DefaultUserID = 1;

        public const decimal DefaultUserBalance = 5000.00m;
        public static int CurrentUserID { get; set; } = DefaultUserID;
        public static bool IsLoggedIn => CurrentUserID > 0;
        public static decimal CurrentUserBalance { get; set; } = DefaultUserBalance;
    }
}