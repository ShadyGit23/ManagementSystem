using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Data;
using ServerApp.DTOs;
using ServerApp.Models;

namespace ServerApp.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthApiController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // POST: /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Пошук користувача в базі даних за Email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());

        if (user == null)
        {
            return Unauthorized(new { message = "Користувача з такою електронною поштою не знайдено." });
        }

        // 2. Перевірка пароля
        // Згідно з пунктом 4 методички, порівнюємо пароль із полем PasswordHash у таблиці Users
        if (user.PasswordHash != dto.Password.Trim())
        {
            return Unauthorized(new { message = "Невірний пароль облікового запису." });
        }

        // 3. Генерація персонального JWT-токена
        var token = GenerateUserJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserID = user.ID,
            Name = user.Name,
            Email = user.Email
        });
    }

    private string GenerateUserJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKeyString = jwtSettings["Key"] ?? "SuperSecretLoLAdminKey2026MatchupArmorySystemVeryLongKey!";
        var secretKeyBytes = Encoding.UTF8.GetBytes(secretKeyString);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.TryParse(jwtSettings["ExpireMinutes"], out var exp) ? exp : 120),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKeyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}