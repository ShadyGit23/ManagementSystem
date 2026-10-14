using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введіть логін")]
    [Display(Name = "Логін адміністратора")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}