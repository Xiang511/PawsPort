namespace PawsPort.Dtos
{
    /// <summary>
    /// 重置密碼 DTO
    /// </summary>
    public class ResetPasswordDTO
    {
        /// <summary>
        /// 用戶郵箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 重置 Token
        /// </summary>
        public string ResetToken { get; set; } = string.Empty;

        /// <summary>
        /// 新密碼
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// 確認新密碼
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
