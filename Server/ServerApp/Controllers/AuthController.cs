using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Models;

namespace ServerApp.Controllers;

public class AuthController : Controller
{
    private readonly IConfiguration _configuration;

    // Пункт 3 методички: фіксований адміністратор у коді
    private const string AdminUser = "admin";
    private const string AdminPass = "Admin123!";

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // GET: /Auth/Login — відкриття форми входу
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    // POST: /Auth/Login (або /auth/login) — перевірка пароля та видача токена
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Перевірка облікових даних адміністратора
        if (model.Username == AdminUser && model.Password == AdminPass)
        {
            var token = GenerateJwtToken(model.Username);

            // Зберігаємо токен у Cookie для комфортної роботи у веб-браузері
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Для локальної розробки
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            return RedirectToAction("Index", "Admin");
        }

        ModelState.AddModelError(string.Empty, "Невірний логін або пароль!");
        return View(model);
    }

    // GET: /Auth/Logout
    [HttpGet]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");
        return RedirectToAction("Login");
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}