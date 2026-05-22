namespace PawsPort.Dtos
{
    /// <summary>
    /// 登入記錄 DTO（包含使用者名稱）
    /// </summary>
    public class LoginActivityWithUserDTO
    {
        public int LogId { get; set; }

        public DateTime? LoginTime { get; set; }

        public string Ipaddress { get; set; }

        public string DeviceInfo { get; set; }

        public string AuthType { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool? Status { get; set; }

        public int? UserId { get; set; }

        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UserName { get; set; }
    }
}
