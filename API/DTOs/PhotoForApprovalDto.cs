namespace API.DTOs;

public class PhotoForApprovalDto
{
    //Photo Id, the Url, the Username and the isApproved status
    public int Id { get; set; }
    public required string Url { get; set; }
    public string? Username { get; set; } // optional as this matches the AppUser entity prop
    public bool IsApproved { get; set; }
}