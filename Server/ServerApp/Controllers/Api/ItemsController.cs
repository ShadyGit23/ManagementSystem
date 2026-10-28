using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.DTOs;

namespace ServerApp.Controllers.Api;

/// <summary>
/// REST API контролер для надання даних про чемпіонів та їх збірки.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItemsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримати повний каталог чемпіонів (Оптимізовано через AsNoTracking).
    /// </summary>
    /// <returns>Список DTO об'єктів чемпіонів із білдами та рунами.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ItemResponseDto>>> GetItems()
    {
        // Оптимізація Завдання 11: AsNoTracking() вимикає ChangeTracker для швидких вибірок
        var items = await _context.Items
            .AsNoTracking()
            .Include(i => i.User)
            .Select(i => new ItemResponseDto
            {
                ID = i.ID,
                ChampionName = i.ChampionName,
                Role = i.Role,
                CoreItemBuild = i.CoreItemBuild,
                RuneSetup = i.RuneSetup,
                UserID = i.UserID,
                AuthorName = i.User != null ? i.User.Name : "Невідомий"
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Отримати деталі конкретного чемпіона за первинним ключем ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemResponseDto>> GetItem(int id)
    {
        var item = await _context.Items
            .AsNoTracking()
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.ID == id);

        if (item == null)
        {
            return NotFound(new { message = $"Сетап чемпіона з ID {id} не знайдено." });
        }

        var dto = new ItemResponseDto
        {
            ID = item.ID,
            ChampionName = item.ChampionName,
            Role = item.Role,
            CoreItemBuild = item.CoreItemBuild,
            RuneSetup = item.RuneSetup,
            UserID = item.UserID,
            AuthorName = item.User?.Name ?? "Невідомий"
        };

        return Ok(dto);
    }
}