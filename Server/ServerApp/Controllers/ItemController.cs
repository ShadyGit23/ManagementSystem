using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.Models;

namespace ServerApp.Controllers;

[Authorize]
public class ItemsController : Controller
{
    private readonly AppDbContext _context;

    public ItemsController(AppDbContext context)
    {
        _context = context;
    }

    // READ: Список тактичних сетапів з даними авторів
    public async Task<IActionResult> Index()
    {
        var items = await _context.Items.Include(i => i.User).ToListAsync();
        return View(items);
    }

    // CREATE: Форма
    public async Task<IActionResult> Create()
    {
        ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "ID", "Name");
        return View();
    }

    // CREATE: Збереження
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Item item)
    {
        if (ModelState.IsValid)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "ID", "Name", item.UserID);
        return View(item);
    }

    // UPDATE: Форма
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();

        ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "ID", "Name", item.UserID);
        return View(item);
    }

    // UPDATE: Збереження
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Item item)
    {
        if (id != item.ID) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "ID", "Name", item.UserID);
        return View(item);
    }

    // DELETE: Форма
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.Items
            .Include(i => i.User)
            .FirstOrDefaultAsync(m => m.ID == id);

        if (item == null) return NotFound();

        return View(item);
    }

    // DELETE: Підтвердження
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}