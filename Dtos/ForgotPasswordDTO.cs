namespace PawsPort.Dtos
{
    /// <summary>
    /// 請求密碼重置 DTO
    /// </summary>
    public class ForgotPasswordDTO
    {
        /// <summary>
        /// 用戶郵箱
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}
