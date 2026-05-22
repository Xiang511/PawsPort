using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using Serilog;
using System.Text.Json;

namespace PawsPort.Services
{
    /// <summary>
    /// 登入記錄服務
    /// </summary>
    public class LoginLogService
    {
        private readonly PetDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public LoginLogService(PetDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// 記錄使用者登入
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="email">使用者 Email</param>
        /// <param name="ipAddress">IP 位址</param>
        /// <param name="userAgent">瀏覽器 User Agent</param>
        /// <returns></returns>
        public async Task<bool> LogUserLoginAsync(int userId, string email, string ipAddress, string userAgent)
        {
            try
            {
                Log.Information("[LoginLogService] 開始記錄登入，UserId: {UserId}, IP: {IP}", userId, ipAddress);

                // 取得 IP 地理位置資訊
                var locationInfo = await GetIpLocationAsync(ipAddress);

                // 建立登入記錄
                var loginLog = new LoginActivity
                {
                    UserId = userId,
                    Ipaddress = ipAddress,
                    DeviceInfo = userAgent,
                    AuthType = "JWT", // 可根據實際情況調整
                    Country = locationInfo?.CountryName,
                    City = locationInfo?.City,
                    Latitude = locationInfo?.Latitude != null ? (decimal)locationInfo.Latitude : null,
                    Longitude = locationInfo?.Longitude != null ? (decimal)locationInfo.Longitude : null,
                    LoginTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time")),
                    Status = true // true = 成功
                };

                _context.LoginActivities.Add(loginLog);
                await _context.SaveChangesAsync();

                Log.Information("[LoginLogService] 登入記錄成功，LogId: {LogId}", loginLog.LogId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[LoginLogService] 記錄登入失敗，UserId: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// 記錄失敗的登入嘗試
        /// </summary>
        public async Task<bool> LogFailedLoginAsync(string email, string ipAddress, string userAgent, string failureReason)
        {
            try
            {
                var locationInfo = await GetIpLocationAsync(ipAddress);

                var loginLog = new LoginActivity
                {
                    UserId = null, // 失敗的登入可能沒有 UserId
                    Ipaddress = ipAddress,
                    DeviceInfo = $"{userAgent} | Reason: {failureReason}",
                    AuthType = "JWT",
                    Country = locationInfo?.CountryName,
                    City = locationInfo?.City,
                    Latitude = locationInfo?.Latitude != null ? (decimal)locationInfo.Latitude : null,
                    Longitude = locationInfo?.Longitude != null ? (decimal)locationInfo.Longitude : null,
                    LoginTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time")),
                    Status = false // false = 失敗
                };

                _context.LoginActivities.Add(loginLog);
                await _context.SaveChangesAsync();

                Log.Warning("[LoginLogService] 記錄失敗登入，Email: {Email}, Reason: {Reason}", email, failureReason);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[LoginLogService] 記錄失敗登入時發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 使用 ipstack API 取得 IP 地理位置資訊
        /// </summary>
        private async Task<IpStackResponseDTO?> GetIpLocationAsync(string ipAddress)
        {
            try
            {
                // 跳過本地 IP
                if (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress.StartsWith("192.168."))
                {
                    Log.Debug("[LoginLogService] 偵測到本地 IP: {IP}，跳過地理位置查詢", ipAddress);
                    return null;
                }

                var apiKey = _configuration["IPSTACK:Key"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    Log.Warning("[LoginLogService] IpStack API Key 未設定");
                    return null;
                }

                var url = $"http://api.ipstack.com/{ipAddress}?access_key={apiKey}";

                Log.Debug("[LoginLogService] 呼叫 ipstack API: {IP}", ipAddress);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<IpStackResponseDTO>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Log.Debug("[LoginLogService] ipstack 回應: {Country}, {City}", result?.CountryName, result?.City);
                return result;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "[LoginLogService] 呼叫 ipstack API 失敗");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[LoginLogService] 解析 ipstack 回應失敗");
                return null;
            }
        }

        /// <summary>
        /// 取得使用者的登入歷史記錄
        /// </summary>
        public async Task<List<LoginActivityWithUserDTO>> GetUserLoginHistoryAsync(int userId, int limit = 10)
        {
            return await _context.LoginActivities
                .Where(log => log.UserId == userId)
                .Join(_context.UserTables,
                    log => log.UserId,
                    user => user.UserId,
                    (log, user) => new LoginActivityWithUserDTO
                    {
                        LogId = log.LogId,
                        LoginTime = log.LoginTime,
                        Ipaddress = log.Ipaddress,
                        DeviceInfo = log.DeviceInfo,
                        AuthType = log.AuthType,
                        Country = log.Country,
                        City = log.City,
                        Latitude = log.Latitude,
                        Longitude = log.Longitude,
                        Status = log.Status,
                        UserId = log.UserId,
                        UserName = user.Name
                    })
                .OrderByDescending(log => log.LoginTime)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// 取得最近的失敗登入嘗試
        /// </summary>
        public async Task<List<LoginActivityWithUserDTO>> GetRecentFailedLoginsAsync(string email, TimeSpan timeWindow)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);

            return await _context.LoginActivities
                .Where(log => log.Status == false && 
                             log.LoginTime >= cutoffTime)
                .GroupJoin(_context.UserTables,
                    log => log.UserId,
                    user => user.UserId,
                    (log, users) => new { log, users })
                .SelectMany(
                    x => x.users.DefaultIfEmpty(),
                    (x, user) => new LoginActivityWithUserDTO
                    {
                        LogId = x.log.LogId,
                        LoginTime = x.log.LoginTime,
                        Ipaddress = x.log.Ipaddress,
                        DeviceInfo = x.log.DeviceInfo,
                        AuthType = x.log.AuthType,
                        Country = x.log.Country,
                        City = x.log.City,
                        Latitude = x.log.Latitude,
                        Longitude = x.log.Longitude,
                        Status = x.log.Status,
                        UserId = x.log.UserId,
                        UserName = user != null ? user.Name : null
                    })
                .OrderByDescending(log => log.LoginTime)
                .ToListAsync();
        }

        /// <summary>
        /// 取得所有成功的登入記錄
        /// </summary>
        /// <param name="skip">略過的筆數（分頁用）</param>
        /// <param name="take">取得的筆數（分頁用）</param>
        /// <returns>成功的登入記錄列表</returns>
        public async Task<List<LoginActivityWithUserDTO>> GetAllSuccessfulLoginsAsync(int skip = 0, int take = 100)
        {
            return await _context.LoginActivities
                .Where(log => log.Status == true)
                .GroupJoin(_context.UserTables,
                    log => log.UserId,
                    user => user.UserId,
                    (log, users) => new { log, users })
                .SelectMany(
                    x => x.users.DefaultIfEmpty(),
                    (x, user) => new LoginActivityWithUserDTO
                    {
                        LogId = x.log.LogId,
                        LoginTime = x.log.LoginTime,
                        Ipaddress = x.log.Ipaddress,
                        DeviceInfo = x.log.DeviceInfo,
                        AuthType = x.log.AuthType,
                        Country = x.log.Country,
                        City = x.log.City,
                        Latitude = x.log.Latitude,
                        Longitude = x.log.Longitude,
                        Status = x.log.Status,
                        UserId = x.log.UserId,
                        UserName = user != null ? user.Name : null
                    })
                .OrderByDescending(log => log.LoginTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// 取得所有失敗的登入記錄
        /// </summary>
        /// <param name="skip">略過的筆數（分頁用）</param>
        /// <param name="take">取得的筆數（分頁用）</param>
        /// <returns>失敗的登入記錄列表</returns>
        public async Task<List<LoginActivityWithUserDTO>> GetAllFailedLoginsAsync(int skip = 0, int take = 100)
        {
            return await _context.LoginActivities
                .Where(log => log.Status == false)
                .GroupJoin(_context.UserTables,
                    log => log.UserId,
                    user => user.UserId,
                    (log, users) => new { log, users })
                .SelectMany(
                    x => x.users.DefaultIfEmpty(),
                    (x, user) => new LoginActivityWithUserDTO
                    {
                        LogId = x.log.LogId,
                        LoginTime = x.log.LoginTime,
                        Ipaddress = x.log.Ipaddress,
                        DeviceInfo = x.log.DeviceInfo,
                        AuthType = x.log.AuthType,
                        Country = x.log.Country,
                        City = x.log.City,
                        Latitude = x.log.Latitude,
                        Longitude = x.log.Longitude,
                        Status = x.log.Status,
                        UserId = x.log.UserId,
                        UserName = user != null ? user.Name : null
                    })
                .OrderByDescending(log => log.LoginTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// 取得成功登入記錄的總數
        /// </summary>
        public async Task<int> GetSuccessfulLoginsCountAsync()
        {
            return await _context.LoginActivities
                .Where(log => log.Status == true)
                .CountAsync();
        }

        /// <summary>
        /// 取得失敗登入記錄的總數
        /// </summary>
        public async Task<int> GetFailedLoginsCountAsync()
        {
            return await _context.LoginActivities
                .Where(log => log.Status == false)
                .CountAsync();
        }
    }
}
