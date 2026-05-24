namespace PawsPort.Dtos
{
    /// <summary>
    /// 請求發送驗證碼 DTO
    /// </summary>
    public class RequestVerificationCodeDTO
    {
        /// <summary>
        /// 用戶郵箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密碼（用於初步驗證）
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
