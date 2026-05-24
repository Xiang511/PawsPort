namespace PawsPort.Dtos
{
    /// <summary>
    /// 驗證重置 Token DTO
    /// </summary>
    public class VerifyResetTokenDTO
    {
        /// <summary>
        /// 用戶郵箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 重置 Token
        /// </summary>
        public string ResetToken { get; set; } = string.Empty;
    }
}
