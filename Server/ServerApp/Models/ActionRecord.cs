using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("Actions")]
public class ActionRecord
{
    [Key]
    public int ID { get; set; }

    [Required]
    public string OpponentChampion { get; set; } = string.Empty;

    [Required]
    public string ActionDetails { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Draft"; // Draft, Approved, Archived

    // Зовнішні ключі
    public int UserID { get; set; }
    [ForeignKey(nameof(UserID))]
    public User? User { get; set; }

    public int ItemID { get; set; }
    [ForeignKey(nameof(ItemID))]
    public Item? Item { get; set; }
}