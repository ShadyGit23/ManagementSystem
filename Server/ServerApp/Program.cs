using Microsoft.EntityFrameworkCore;
using ServerApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Реєстрація AppDbContext з SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Підтримка контролерів (знадобиться для API та MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapGet("/", () => "League of Legends Matchup & Armory System API is running!");
app.MapDefaultControllerRoute();

app.Run();