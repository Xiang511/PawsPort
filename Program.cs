using Microsoft.EntityFrameworkCore;
using PawsPort.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 註冊資料庫連線
// 優先順序: User Secrets > 環境變數 > appsettings.json
string? isLocal = builder.Configuration["IS_LOCAL"];
string connectionString;

if (isLocal == "true")
{
    // 本地開發環境使用 Windows 驗證
    connectionString = "Data Source=.;Initial Catalog=PetDB;Integrated Security=True;Encrypt=False";
}
else
{
    // 生產環境或遠端資料庫
    connectionString = builder.Configuration["PETDB"]
        ?? throw new InvalidOperationException("找不到資料庫連接字串。請設定 User Secrets 或環境變數。");
}

builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlServer(connectionString));

// 設定 Serilog
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()

    // 1. Console 輸出（開發環境用）
    .WriteTo.Console()

    // 2. 一般日誌：每天分檔 + 大小限制 (10MB)
    .WriteTo.File(
        path: "logs/all-log-.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,          // 開啟大小限制分檔
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
        retainedFileCountLimit: 31,         // 保留最近 31 個檔案
        shared: true                        // 若有多個程序讀取建議加上
    )

    // 3. 錯誤日誌：篩選 Error 以上 + 大小限制 (5MB)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
        .WriteTo.File(
            path: "logs/only-errors-.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 5 * 1024 * 1024, // 錯誤日誌通常較小，設 5MB
            retainedFileCountLimit: null         // 不限制數量，確保錯誤記錄不丟失
        )
    );

// 4. Seq 伺服器（根據環境變數決定是否啟用）
string? seqEnabled = builder.Configuration["SEQ_ENABLED"];
if (seqEnabled?.ToLower() == "true")
{
    string seqUrl = "http://localhost:5341";
    loggerConfig.WriteTo.Seq(seqUrl);
    Console.WriteLine($" Seq 日誌已啟用: {seqUrl}");
}
else
{
    Console.WriteLine(" Seq 日誌未啟用");
}

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();


app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Member}/{action=List}/{id?}")
    .WithStaticAssets();


app.Run();
