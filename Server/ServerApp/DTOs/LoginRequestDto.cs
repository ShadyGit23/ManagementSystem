using System.ComponentModel.DataAnnotations;

namespace ServerApp.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Вкажіть Email адресу")]
    [EmailAddress(ErrorMessage = "Некоректний формат Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    public string Password { get; set; } = string.Empty;
}