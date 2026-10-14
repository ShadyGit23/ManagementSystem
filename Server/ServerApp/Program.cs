using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. БАЗА ДАНИХ (SQLite)
// ==========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=app.db"));

// ==========================================
// 2. MVC КОНТРОЛЕРИ ТА JSON СЕРІАЛІЗАЦІЯ
// ==========================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Запобігає зацикленню при серіалізації навігаційних зв'язків EF Core
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ==========================================
// 3. JWT-АУТЕНТИФІКАЦІЯ
// ==========================================
// Читаємо той самий ключ, що і в AuthController (із appsettings.json з надійним fallback)
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKeyString = jwtSettings["Key"] ?? "SuperSecretLoLAdminKey2026MatchupArmorySystemVeryLongKey!";
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKeyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,   // Вимикаємо сувору перевірку для локальної розробки
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Зчитуємо саме ту cookie "jwt", яку записує AuthController
            if (context.Request.Cookies.TryGetValue("jwt", out var jwtToken))
            {
                context.Token = jwtToken;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Для API повертаємо 401, для браузера — редирект на логін
            if (!context.Request.Path.StartsWithSegments("/api") && !context.Response.HasStarted)
            {
                context.HandleResponse();
                context.Response.Redirect("/Auth/Login");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ==========================================
// 4. SWAGGER ДЛЯ REST API
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 5. КОНФІГУРАЦІЯ HTTP-КОНВЕЄРА (MIDDLEWARE)
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Якщо користувач уже авторизований — відкриваємо дашборд адмінки, інакше — форму логіну
app.MapGet("/", (HttpContext ctx) => 
    ctx.User.Identity?.IsAuthenticated == true 
        ? Results.Redirect("/Admin") 
        : Results.Redirect("/Auth/Login"));

// Маршрутизація для MVC адмін-панелі
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

// Маршрутизація для REST API контролерів
app.MapControllers();

app.Run();