using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Group
{
    [Key] // tell EF the name provided is unique, if prop with name Id is created - it automatically will be picked for unique key
    public required string Name { get; set; }

    public ICollection<Connection> Connections {get; set; } = [];
}