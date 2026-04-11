using Microsoft.EntityFrameworkCore;
using PawsPort.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();



string connectionString = builder.Configuration["PetDB"]
    ?? throw new InvalidOperationException("DB connection string not found.");

builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlServer(connectionString));


Log.Logger = (Serilog.ILogger)new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .WriteTo.File("logs/all-log-.txt", rollingInterval: RollingInterval.Day) 
    .WriteTo.Seq("http://localhost:5341")


    // 1. 一般日誌：每天分檔 + 大小限制 (10MB)
    .WriteTo.File(
        path: "logs/all-log-.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,          // 開啟大小限制分檔
        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
        retainedFileCountLimit: 31,         // 保留最近 31 個檔案
        shared: true                        // 若有多個程序讀取建議加上
    )

    // 2. 錯誤日誌：篩選 Error 以上 + 大小限制 (5MB)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
        .WriteTo.File(
            path: "logs/only-errors-.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 5 * 1024 * 1024, // 錯誤日誌通常較小，設 5MB
            retainedFileCountLimit: null         // 不限制數量，確保錯誤記錄不丟失
        )
    )
    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
    .WriteTo.File("logs/only-errors-.txt", rollingInterval: RollingInterval.Day)
    //.WriteTo.Seq("http://localhost:5341") 
    .CreateLogger();

builder.Host.UseSerilog(); 



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Member}/{action=List}/{id?}")
    .WithStaticAssets();


app.Run();
