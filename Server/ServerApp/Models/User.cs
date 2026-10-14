using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("Users")]
public class User
{
    [Key]
    public int ID { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Навігаційні властивості (зв'язки один-до-багатьох)
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<ActionRecord> Actions { get; set; } = new List<ActionRecord>();
}