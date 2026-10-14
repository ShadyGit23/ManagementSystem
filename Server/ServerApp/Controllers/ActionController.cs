using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.Models;

namespace ServerApp.Controllers;

[Authorize]
public class ActionsController : Controller
{
    private readonly AppDbContext _context;

    public ActionsController(AppDbContext context)
    {
        _context = context;
    }

    // READ: Список гайдів з зв'язаними User та Item
    public async Task<IActionResult> Index()
    {
        var actions = await _context.Actions
            .Include(a => a.User)
            .Include(a => a.Item)
            .ToListAsync();

        return View(actions);
    }

    // CREATE: Форма
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    // CREATE: Збереження
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActionRecord actionRecord)
    {
        if (ModelState.IsValid)
        {
            _context.Actions.Add(actionRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdownsAsync(actionRecord.UserID, actionRecord.ItemID);
        return View(actionRecord);
    }

    // UPDATE: Форма
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var actionRecord = await _context.Actions.FindAsync(id);
        if (actionRecord == null) return NotFound();

        await PopulateDropdownsAsync(actionRecord.UserID, actionRecord.ItemID);
        return View(actionRecord);
    }

    // UPDATE: Збереження
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActionRecord actionRecord)
    {
        if (id != actionRecord.ID) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(actionRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdownsAsync(actionRecord.UserID, actionRecord.ItemID);
        return View(actionRecord);
    }

    // DELETE: Форма
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var actionRecord = await _context.Actions
            .Include(a => a.User)
            .Include(a => a.Item)
            .FirstOrDefaultAsync(m => m.ID == id);

        if (actionRecord == null) return NotFound();

        return View(actionRecord);
    }

    // DELETE: Підтвердження
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var actionRecord = await _context.Actions.FindAsync(id);
        if (actionRecord != null)
        {
            _context.Actions.Remove(actionRecord);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(int? selectedUser = null, int? selectedItem = null)
    {
        ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "ID", "Name", selectedUser);
        ViewBag.Items = new SelectList(await _context.Items.ToListAsync(), "ID", "ChampionName", selectedItem);
    }
}