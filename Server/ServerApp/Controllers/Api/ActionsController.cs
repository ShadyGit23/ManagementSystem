using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.DTOs;
using ServerApp.Models;

namespace ServerApp.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Захист ендпоінту згідно з пунктом 5 методички
public class ActionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActionsController(AppDbContext context)
    {
        _context = context;
    }

    // POST: /api/actions
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAction([FromBody] ActionCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Перевірка існування зв'язаного чемпіона (Item)
        var itemExists = await _context.Items.AnyAsync(i => i.ID == dto.ItemID);
        if (!itemExists)
        {
            return BadRequest(new { message = $"Помилка: чемпіона (Item) з ID {dto.ItemID} не існує в базі даних." });
        }

        // Перевірка існування користувача (User)
        var userExists = await _context.Users.AnyAsync(u => u.ID == dto.UserID);
        if (!userExists)
        {
            return BadRequest(new { message = $"Помилка: користувача (User) з ID {dto.UserID} не існує." });
        }

        var actionRecord = new ActionRecord
        {
            ItemID = dto.ItemID,
            UserID = dto.UserID,
            OpponentChampion = dto.OpponentChampion.Trim(),
            ActionDetails = dto.ActionDetails.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Draft" : dto.Status
        };

        _context.Actions.Add(actionRecord);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActionById), new { id = actionRecord.ID }, actionRecord);
    }

    // GET: /api/actions/5 (для підтримки заголовка Location у CreatedAtAction)
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActionById(int id)
    {
        var action = await _context.Actions
            .Include(a => a.Item)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.ID == id);

        if (action == null)
        {
            return NotFound();
        }

        return Ok(action);
    }
}