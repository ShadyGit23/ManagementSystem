using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;

namespace ServerApp.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UsersCount = await _context.Users.CountAsync();
        ViewBag.ItemsCount = await _context.Items.CountAsync();
        ViewBag.ActionsCount = await _context.Actions.CountAsync();

        return View();
    }
}