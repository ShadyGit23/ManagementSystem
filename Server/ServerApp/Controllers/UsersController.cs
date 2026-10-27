using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.Models;

namespace ServerApp.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // READ: Список користувачів
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    // CREATE: Форма
    public IActionResult Create()
    {
        return View();
    }

    // CREATE: Збереження
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            // Хешуємо пароль алгоритмом BCrypt із автоматичною сіллю
            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.PasswordHash.Trim());

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // UPDATE: Форма
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }

    // UPDATE: Збереження
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.ID) return NotFound();

        if (ModelState.IsValid)
        {

            // Якщо введено новий звичайний текст (не хеш BCrypt), хешуємо його
            // Хеші BCrypt завжди починаються з $2a$, $2b$ або $2y$
            if (!user.PasswordHash.StartsWith("$2"))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.PasswordHash.Trim());
            }
            
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // DELETE: Підтвердження
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.Users.FirstOrDefaultAsync(m => m.ID == id);
        if (user == null) return NotFound();

        return View(user);
    }

    // DELETE: Фізичне видалення
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}