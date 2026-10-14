using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("Items")]
public class Item
{
    [Key]
    public int ID { get; set; }

    [Required]
    public string ChampionName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string CoreItemBuild { get; set; } = string.Empty;

    [Required]
    public string RuneSetup { get; set; } = string.Empty;

    // Зовнішній ключ
    public int UserID { get; set; }
    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }

    // Зв'язок із гайдами дій
    public ICollection<ActionRecord> Actions { get; set; } = new List<ActionRecord>();
}