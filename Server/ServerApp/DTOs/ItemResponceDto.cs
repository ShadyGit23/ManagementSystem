namespace ServerApp.DTOs;

public class ItemResponseDto
{
    public int ID { get; set; }
    public string ChampionName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string CoreItemBuild { get; set; } = string.Empty;
    public string RuneSetup { get; set; } = string.Empty;
    public int UserID { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}