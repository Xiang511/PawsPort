namespace PawsPort.Dtos
{
    /// <summary>
    /// 驗證碼驗證 DTO
    /// </summary>
    public class VerifyCodeDTO
    {
        /// <summary>
        /// 用戶郵箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 驗證碼
        /// </summary>
        public string VerificationCode { get; set; } = string.Empty;
    }
}
