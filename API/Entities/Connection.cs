namespace API.Entities;

public class Connection
{
    //by having Id in the end of the name of the prop tells EF to use it as a key
    public required string ConnectionId { get; set; } 
    public required string Username { get; set; }
}