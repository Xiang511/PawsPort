using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PawsPort.Authorization;
using PawsPort.Middlewares;
using PawsPort.Models;
using PawsPort.Services;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. 讀取 JWT 設定
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

// 2. 設定驗證服務
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 先從 Authorization Header 讀取
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            // 如果 Header 没有，讀 Cookie 
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["X-Access-Token"];
            }

            context.Token = token;
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero // 取消預設的 5 分鐘緩衝時間，讓過期更精準
    };
});

// 3. 設定授權策略（使用擴展方法）
builder.Services.AddPawsPortAuthorization();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PawsPortPolicy", policy =>
    {
        // 允許你的 Vue 前端網址
        policy.WithOrigins("https://localhost:5173")
        .WithOrigins("https://localhost:5174")
        .WithOrigins("https://localhost:5175")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();


//auth
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LoginLogService>();
builder.Services.AddScoped<GoogleOAuthService>();

// 註冊 HttpClient
builder.Services.AddHttpClient();




// member
builder.Services.AddScoped<MemberProfileService>();
builder.Services.AddScoped<MemberPermissionService>();
builder.Services.AddScoped<MemberBlockListService>();

// community
builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CommentService>();

// game
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<QuestionsService>();
builder.Services.AddScoped<ShopService>();

// pet
builder.Services.AddScoped<PassPortService>();
builder.Services.AddScoped<PetService>();
builder.Services.AddScoped<PetAdoptionService>();
builder.Services.AddScoped<AdoptionRecordService>();
builder.Services.AddScoped<MissingReportsService>();
builder.Services.AddScoped<ClientMissingPetService>();
builder.Services.AddScoped<PetPassportService>();

// support
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


// 設定 Serilog - 完全從 appsettings.json 讀取
// Service DI


// 設定 Serilog
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

        // 設定 Tags 順序（依照模組分類）
        var tagOrder = new[]
        {
            "會員管理",
            "權限管理",
            "寵物管理",
            "遊戲系統",
            "社群管理",
            "客服管理"
        };

        // 對現有的 Tags 進行排序
        if (document.Tags != null && document.Tags.Count > 0)
        {
            var sortedTags = document.Tags
                .OrderBy(tag =>
                {
                    int index = Array.IndexOf(tagOrder, tag.Name);
                    return index == -1 ? int.MaxValue : index;
                })
                .ToList();

            document.Tags.Clear();
            foreach (var tag in sortedTags)
            {
                document.Tags.Add(tag);
            }
        }

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


app.UseCors("PawsPortPolicy"); // 啟用 CORS
app.UseAuthentication(); // 認證：你是誰？
app.UseAuthorization();  // 授權：你能做什麼？



app.MapStaticAssets();
// 根路徑重定向到 Scalar API 文件  ← 新增這兩行
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
