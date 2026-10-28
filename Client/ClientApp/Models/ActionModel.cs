namespace ClientApp.Models;

public class ActionModel
{
    public int ItemID { get; set; }
    public int UserID { get; set; }
    public string OpponentChampion { get; set; } = string.Empty;
    public string ActionDetails { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
}