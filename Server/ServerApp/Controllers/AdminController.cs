using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServerApp.Controllers;

[Authorize]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return Content($"Панель адміністратора активна! Ви успішно пройшли JWT-аутентифікацію як: {User.Identity?.Name}. <br><a href='/Auth/Logout'>Вийти</a>", "text/html; charset=utf-8");
    }
}