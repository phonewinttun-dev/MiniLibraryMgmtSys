namespace MiniLibraryMgmtSys.Services
{
    public class UserServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static UserServiceResult<T> Success(T data, string message = "Success") =>
            new() { IsSuccess = true, Data = data, Message = message };

        public static UserServiceResult<T> Failure(string message) =>
            new() { IsSuccess = false, Message = message };
    }
}
