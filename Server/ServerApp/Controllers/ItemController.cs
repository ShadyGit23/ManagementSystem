using System.Text.Json;
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
    private readonly IWebHostEnvironment _env;

    public ItemsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
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

    // =========================================================================
    // ⚡ ФУНКЦІЇ АВТОМАТИЧНОГО ТА ФАЙЛОВОГО ІМПОРТУ РОСТЕРУ MOBALYTICS
    // =========================================================================

    // 1. Швидкий імпорт у 1 клік (зчитує готовий Database/champions_mobalytics.json)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedMobalytics()
    {
        var defaultUser = await _context.Users.FirstOrDefaultAsync();
        if (defaultUser == null)
        {
            TempData["ErrorMessage"] = "Помилка: Спершу створіть хоча б одного користувача в системі!";
            return RedirectToAction(nameof(Index));
        }

        // Шлях до файлу в проєкті або корені репозиторію
        var possiblePaths = new[]
        {
            Path.Combine(_env.ContentRootPath, "..", "..", "Database", "champions_mobalytics.json"),
            Path.Combine(_env.ContentRootPath, "Database", "champions_mobalytics.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "Database", "champions_mobalytics.json")
        };

        string? filePath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p));

        if (filePath == null)
        {
            TempData["ErrorMessage"] = "Файл champions_mobalytics.json не знайдено за стандартним шляхом Database/.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var jsonString = await System.IO.File.ReadAllTextAsync(filePath);
            var seedList = JsonSerializer.Deserialize<List<ChampionSeedDto>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (seedList == null || !seedList.Any())
            {
                TempData["ErrorMessage"] = "Файл JSON порожній або має некоректний формат.";
                return RedirectToAction(nameof(Index));
            }

            // Перевіряємо вже існуючих чемпіонів, щоб не створювати дублікати
            var existingNames = await _context.Items.Select(i => i.ChampionName.ToLower()).ToListAsync();
            var itemsToAdd = new List<Item>();

            foreach (var champ in seedList)
            {
                if (!existingNames.Contains(champ.ChampionName.Trim().ToLower()))
                {
                    itemsToAdd.Add(new Item
                    {
                        ChampionName = champ.ChampionName.Trim(),
                        Role = champ.Role.Trim(),
                        CoreItemBuild = champ.CoreItemBuild.Trim(),
                        RuneSetup = champ.RuneSetup.Trim(),
                        UserID = defaultUser.ID
                    });
                }
            }

            if (itemsToAdd.Any())
            {
                await _context.Items.AddRangeAsync(itemsToAdd);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Успішно імпортовано {itemsToAdd.Count} нових чемпіонів за версією Mobalytics.gg!";
            }
            else
            {
                TempData["InfoMessage"] = "Усі 171 чемпіон уже присутні в базі даних. Дублікатів не створено.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Помилка під час обробки JSON: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // 2. Імпорт користувацького JSON-файлу через веб-форму
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportJsonFile(IFormFile? jsonFile)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Будь ласка, оберіть валідний файл .json для завантаження.";
            return RedirectToAction(nameof(Index));
        }

        var defaultUser = await _context.Users.FirstOrDefaultAsync();
        if (defaultUser == null)
        {
            TempData["ErrorMessage"] = "Помилка: У базі немає жодного користувача для прив'язки авторів!";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using var stream = new StreamReader(jsonFile.OpenReadStream());
            var jsonContent = await stream.ReadToEndAsync();
            var seedList = JsonSerializer.Deserialize<List<ChampionSeedDto>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (seedList == null || !seedList.Any())
            {
                TempData["ErrorMessage"] = "Завантажений JSON-файл не містить записів або має невірну структуру.";
                return RedirectToAction(nameof(Index));
            }

            var existingNames = await _context.Items.Select(i => i.ChampionName.ToLower()).ToListAsync();
            var itemsToAdd = new List<Item>();

            foreach (var champ in seedList)
            {
                if (!existingNames.Contains(champ.ChampionName.Trim().ToLower()))
                {
                    itemsToAdd.Add(new Item
                    {
                        ChampionName = champ.ChampionName.Trim(),
                        Role = champ.Role.Trim(),
                        CoreItemBuild = champ.CoreItemBuild.Trim(),
                        RuneSetup = champ.RuneSetup.Trim(),
                        UserID = defaultUser.ID
                    });
                }
            }

            if (itemsToAdd.Any())
            {
                await _context.Items.AddRangeAsync(itemsToAdd);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Успішно завантажено {itemsToAdd.Count} чемпіонів із файлу!";
            }
            else
            {
                TempData["InfoMessage"] = "Всі записи з наданого файлу вже існують у базі даних.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Не вдалося прочитати файл: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}

// Допоміжний DTO для десеріалізації JSON
public class ChampionSeedDto
{
    public string ChampionName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string CoreItemBuild { get; set; } = string.Empty;
    public string RuneSetup { get; set; } = string.Empty;
}