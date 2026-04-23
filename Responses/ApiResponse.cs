namespace PawsPort.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        //成功回傳
        public static ApiResponse<T> Ok(T data, string message = "Success")
            => new() { Success = true, Code = "SUCCESS", Message = message, Data = data };

        //失敗回傳
        public static ApiResponse<object> Fail(string code, string message)
            => new()
            {
                Success = false,
                Code = code,
                Message = message,
                Data = new string[0] // 強制回傳 JSON 中的 []
            };
    }
}
