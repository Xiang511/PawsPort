using Microsoft.EntityFrameworkCore;
using PawsPort.Models;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// 加入 OpenAPI 支援（Scalar 需要）

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "PawsPort API",
            Version = "v1",
            Description = "PawsPort 寵物管理平台 API 文件"
        };
        return Task.CompletedTask;
    });
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.File(
        path: "logs/all-log-.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        retainedFileCountLimit: 31,
        shared: true
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
        .WriteTo.File(
            path: "logs/only-errors-.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 5 * 1024 * 1024,
            retainedFileCountLimit: null
        )
    )
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// 啟用 OpenAPI 端點
app.MapOpenApi();

// 啟用 Scalar UI（現代化 API 文件介面）
app.MapScalarApiReference();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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