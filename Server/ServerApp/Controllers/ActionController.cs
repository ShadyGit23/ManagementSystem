using System.Text.Json;
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
    private readonly IWebHostEnvironment _env;

    public ActionsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // READ: Список гайдів із прив'язаними сутностями User та Item
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

    // =========================================================================
    // ⚡ ФУНКЦІЇ АВТОМАТИЧНОГО ТА ФАЙЛОВОГО ІМПОРТУ ТАКТИК (ACTIONS)
    // =========================================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedMobalyticsActions()
    {
        var defaultUser = await _context.Users.FirstOrDefaultAsync();
        if (defaultUser == null)
        {
            TempData["ErrorMessage"] = "Помилка: Спершу створіть хоча б одного користувача (User) у системі!";
            return RedirectToAction(nameof(Index));
        }

        var allItems = await _context.Items.ToListAsync();
        if (!allItems.Any())
        {
            TempData["ErrorMessage"] = "Помилка: Таблиця чемпіонів (Items) порожня! Перейдіть на сторінку Items і натисніть 'Автоімпорт Mobalytics'.";
            return RedirectToAction(nameof(Index));
        }

        // Розширений пошук файлу actions_mobalytics.json
        var possiblePaths = new[]
        {
            Path.Combine(_env.ContentRootPath, "..", "..", "Database", "actions_mobalytics.json"),
            Path.Combine(_env.ContentRootPath, "Database", "actions_mobalytics.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "Database", "actions_mobalytics.json"),
            Path.Combine(_env.ContentRootPath, "..", "..", "actions_mobalytics.json"),
            Path.Combine(_env.ContentRootPath, "actions_mobalytics.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "actions_mobalytics.json")
        };

        string? filePath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p));

        if (filePath == null)
        {
            TempData["ErrorMessage"] = "Файл actions_mobalytics.json не знайдено на диску. Переконайтеся, що файл збережено в папці Database/.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var jsonString = await System.IO.File.ReadAllTextAsync(filePath);
            var seedList = JsonSerializer.Deserialize<List<ActionSeedDto>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (seedList == null || !seedList.Any())
            {
                TempData["ErrorMessage"] = "Файл дій порожній або має неправильну структуру.";
                return RedirectToAction(nameof(Index));
            }

            var actionsToAdd = new List<ActionRecord>();

            foreach (var seed in seedList)
            {
                var matchedItem = allItems.FirstOrDefault(i => 
                    i.ChampionName.Equals(seed.ChampionName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (matchedItem == null) continue;

                // Перевірка на дублікат проти зазначеного опонента
                bool alreadyExists = await _context.Actions.AnyAsync(a => 
                    a.ItemID == matchedItem.ID && 
                    a.OpponentChampion.ToLower() == seed.OpponentChampion.Trim().ToLower());

                if (!alreadyExists)
                {
                    actionsToAdd.Add(new ActionRecord
                    {
                        ItemID = matchedItem.ID,
                        UserID = defaultUser.ID,
                        OpponentChampion = seed.OpponentChampion.Trim(),
                        ActionDetails = seed.ActionDetails.Trim(),
                        Status = string.IsNullOrWhiteSpace(seed.Status) ? "Approved" : seed.Status.Trim()
                    });
                }
            }

            if (actionsToAdd.Any())
            {
                await _context.Actions.AddRangeAsync(actionsToAdd);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Успішно імпортовано {actionsToAdd.Count} тактичних гайдів проти опонентів із Mobalytics!";
            }
            else
            {
                TempData["InfoMessage"] = "Усі зазначені тактики вже існують у базі даних. Дублікатів не створено.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Помилка імпорту тактик: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportJsonFile(IFormFile? jsonFile)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Оберіть валідний файл формату .json.";
            return RedirectToAction(nameof(Index));
        }

        var defaultUser = await _context.Users.FirstOrDefaultAsync();
        if (defaultUser == null)
        {
            TempData["ErrorMessage"] = "Помилка: У базі відсутній користувач для прив'язки автора!";
            return RedirectToAction(nameof(Index));
        }

        var allItems = await _context.Items.ToListAsync();
        if (!allItems.Any())
        {
            TempData["ErrorMessage"] = "Помилка: Спочатку заповніть таблицю Items!";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using var reader = new StreamReader(jsonFile.OpenReadStream());
            var jsonContent = await reader.ReadToEndAsync();
            var seedList = JsonSerializer.Deserialize<List<ActionSeedDto>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (seedList == null || !seedList.Any())
            {
                TempData["ErrorMessage"] = "Файл не містить коректних записів тактик.";
                return RedirectToAction(nameof(Index));
            }

            var actionsToAdd = new List<ActionRecord>();

            foreach (var seed in seedList)
            {
                var matchedItem = allItems.FirstOrDefault(i => 
                    i.ChampionName.Equals(seed.ChampionName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (matchedItem == null) continue;

                bool alreadyExists = await _context.Actions.AnyAsync(a => 
                    a.ItemID == matchedItem.ID && 
                    a.OpponentChampion.ToLower() == seed.OpponentChampion.Trim().ToLower());

                if (!alreadyExists)
                {
                    actionsToAdd.Add(new ActionRecord
                    {
                        ItemID = matchedItem.ID,
                        UserID = defaultUser.ID,
                        OpponentChampion = seed.OpponentChampion.Trim(),
                        ActionDetails = seed.ActionDetails.Trim(),
                        Status = string.IsNullOrWhiteSpace(seed.Status) ? "Approved" : seed.Status.Trim()
                    });
                }
            }

            if (actionsToAdd.Any())
            {
                await _context.Actions.AddRangeAsync(actionsToAdd);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Успішно завантажено {actionsToAdd.Count} тактичних гайдів із файлу!";
            }
            else
            {
                TempData["InfoMessage"] = "Всі тактики з файлу вже присутні в базі даних.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Помилка читання файлу: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(int? selectedUser = null, int? selectedItem = null)
    {
        ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "ID", "Name", selectedUser);
        ViewBag.Items = new SelectList(await _context.Items.ToListAsync(), "ID", "ChampionName", selectedItem);
    }
}

public class ActionSeedDto
{
    public string ChampionName { get; set; } = string.Empty;
    public string OpponentChampion { get; set; } = string.Empty;
    public string ActionDetails { get; set; } = string.Empty;
    public string Status { get; set; } = "Approved";
}