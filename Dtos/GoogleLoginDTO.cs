namespace PawsPort.Dtos
{
    /// <summary>
    /// Google OAuth 登入請求 DTO
    /// </summary>
    public class GoogleLoginDTO
    {
        /// <summary>
        /// Google Token（可以是 ID Token 或 Access Token）
        /// </summary>
        public string? IdToken { get; set; }

        /// <summary>
        /// Google Access Token（如果前端使用 Authorization Code Flow）
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// 取得實際的 Token（優先使用 AccessToken，否則使用 IdToken）
        /// </summary>
        public string GetToken() => AccessToken ?? IdToken ?? string.Empty;
    }
}

