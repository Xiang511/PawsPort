using Microsoft.EntityFrameworkCore;
using PawsPort.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // 設定最低紀錄等級
    .Enrich.FromLogContext()    // 記錄更多上下文資訊
    .WriteTo.Debug()
    .WriteTo.File("logs/all-log-.txt", rollingInterval: RollingInterval.Day) // 每天產生一個檔案
    .WriteTo.Logger(lc => lc
    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
    .WriteTo.File("logs/only-errors-.txt", rollingInterval: RollingInterval.Day))
    //.WriteTo.Seq("http://localhost:5341") // 將資料送到 Seq 監控軟體
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
