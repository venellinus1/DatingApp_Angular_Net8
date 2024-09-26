using System.Runtime;

namespace API;

public class UserDTO
{
    public required string Username { get; set; }
    public required string KnownAs { get; set; }
    public required string Token { get; set; }
    public string? PhotoUrl { get; set; }
}