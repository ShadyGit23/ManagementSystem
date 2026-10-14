namespace ServerApp.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int UserID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}