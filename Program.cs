using Microsoft.EntityFrameworkCore;
using PawsPort.Middlewares;
using PawsPort.Models;
using PawsPort.Services;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

//解決CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PawsPortPolicy", policy =>
    {
        // 允許你的 Vue 前端網址
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();



builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CommentService>();

builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<QuestionsService>();
builder.Services.AddScoped<ShopService>();

builder.Services.AddScoped<PassPortService>();
builder.Services.AddScoped<PetService>();
builder.Services.AddScoped<AdoptionRecordService>();
builder.Services.AddScoped<MissingReportsService>();

builder.Services.AddScoped<FaqService>();

builder.Services.AddScoped<QaService>();

builder.Services.AddScoped<ENewsletterService>();

builder.Services.AddScoped<LineBotService>();


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

// Service DI


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


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // 設定 API 基本資訊
        document.Info = new()
        {
            Title = " PawsPort API",
            Version = "v1.0.0",
            Description = """
                ## PawsPort 寵物管理平台 API

                ###  概述
                PawsPort 是一個全方位的寵物管理平台，提供會員管理、寵物資料、領養紀錄、文章發布、商城服務等完整功能。

                ### 🚀 主要功能
                - **會員系統**：完整的會員註冊、登入、資料管理
                - **寵物管理**：寵物資料建檔、健康紀錄追蹤
                - **領養服務**：寵物領養流程管理
                - **內容管理**：文章發布與分類系統
                - **電商功能**：商品、訂單、購物車管理
                - **社群互動**：評論、聊天、社群功能

                ###  認證方式
                本 API 使用 JWT Bearer Token 進行身份驗證。請在請求 Header 中加入：
                ```
                Authorization: Bearer {your_token}
                ```

                ###  回應格式
                所有 API 回應皆採用統一的 JSON 格式：
                ```json
                {
                  "success": true,
                  "message": "操作成功",
                  "data": { ... },
                  "statusCode": 200
                }
                ```

                ###  更多文件
                - [GitHub Repository](https://github.com/Xiang511/PawsPort)
                - [Wiki 文件](https://github.com/Xiang511/PawsPort/wiki)
                - [問題回報](https://github.com/Xiang511/PawsPort/issues)
                """,
            Contact = new()
            {
                Name = "PawsPort Development Team",
                Email = "support@pawsport.com",
                Url = new Uri("https://github.com/Xiang511/PawsPort")
            },
            License = new()
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };



        return Task.CompletedTask;
    });
});



var app = builder.Build();

//解決CORS
app.UseCors("PawsPortPolicy");

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// 啟用 OpenAPI 端點
app.MapOpenApi();

// 啟用 Scalar UI（現代化 API 文件介面）
app.MapScalarApiReference(options =>
{
    options.WithTitle("PawsPort 會員系統 API")
           // 將預設顯示的程式碼切換為 JavaScript 的 Axios
           .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
});
app.UseStaticFiles();


app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();
// 啟用 CORS
app.UseCors("AllowVueApp");

app.UseAuthorization();

app.MapStaticAssets();
// 根路徑重定向到 Scalar API 文件  ← 新增這兩行
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
