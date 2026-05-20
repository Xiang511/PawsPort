using System.Text.Json.Serialization;

namespace PawsPort.Dtos
{
    /// <summary>
    /// Google OAuth 使用者資訊 DTO
    /// </summary>
    public class GoogleUserInfoDTO
    {
        /// <summary>
        /// Google User ID
        /// </summary>
        [JsonPropertyName("sub")]
        public string Sub { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// Email 是否已驗證
        /// </summary>
        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        /// <summary>
        /// 使用者名稱
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 頭像 URL
        /// </summary>
        [JsonPropertyName("picture")]
        public string Picture { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        /// <summary>
        /// 姓氏
        /// </summary>
        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }
    }
}
