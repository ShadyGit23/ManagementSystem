using System.ComponentModel.DataAnnotations;

namespace ServerApp.DTOs;

public class ActionCreateDto
{
    [Required(ErrorMessage = "Необхідно вказати ID обраного чемпіона (Item)")]
    public int ItemID { get; set; }

    [Required(ErrorMessage = "Необхідно вказати ID автора гайду (User)")]
    public int UserID { get; set; }

    [Required(ErrorMessage = "Вкажіть ворожого чемпіона на лінії")]
    [MaxLength(50)]
    public string OpponentChampion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть покроковий тактичний план")]
    public string ActionDetails { get; set; } = string.Empty;

    public string Status { get; set; } = "Draft"; // За замовчуванням гайд створюється як чернетка
}