using Microsoft.EntityFrameworkCore;
using PawsPort.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();


// 註冊資料庫連線
// 優先順序: User Secrets > 環境變數 > appsettings.json
string connectionString = builder.Configuration["PetDB"]
    ?? throw new InvalidOperationException("找不到資料庫連接字串。請設定 User Secrets 或環境變數。");

builder.Services.AddDbContext<PetDbContext>(options =>
    options.UseSqlServer(connectionString));


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() 
    .Enrich.FromLogContext()    
    .WriteTo.Debug()
    .WriteTo.File("logs/all-log-.txt", rollingInterval: RollingInterval.Day) 
    .WriteTo.Logger(lc => lc
    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
    .WriteTo.File("logs/only-errors-.txt", rollingInterval: RollingInterval.Day))
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
