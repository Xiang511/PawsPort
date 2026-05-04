using Microsoft.EntityFrameworkCore;
using PawsPort.Middlewares;
using PawsPort.Models;
using PawsPort.Services;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// 註冊應用程式服務
builder.Services.AddScoped<MemberProfileService>();
builder.Services.AddScoped<MemberPermissionService>();
builder.Services.AddScoped<MemberBlockListService>();
// 處理CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // 這是你的 Vue 預設埠號
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



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


// 設定 Serilog - 完全從 appsettings.json 讀取
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext();

// Seq 可以保留在程式碼中動態控制
string? seqEnabled = builder.Configuration["SEQ_ENABLED"];
if (seqEnabled?.ToLower() == "true")
{
    loggerConfig.WriteTo.Seq("http://localhost:5341");
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
