namespace PawsPort.Dtos
{
    /// <summary>
    /// 驗證註冊 Email 驗證碼 DTO
    /// </summary>
    public class RegisterVerifyDTO
    {
        /// <summary>
        /// 用戶郵箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 6 位數驗證碼
        /// </summary>
        public string VerificationCode { get; set; } = string.Empty;
    }
}
