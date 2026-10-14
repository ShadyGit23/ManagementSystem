using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.DTOs;

namespace ServerApp.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Захист ендпоінту згідно з пунктом 5 методички
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItemsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/items
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ItemResponseDto>>> GetItems()
    {
        var items = await _context.Items
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

    // GET: /api/items/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemResponseDto>> GetItem(int id)
    {
        var item = await _context.Items
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