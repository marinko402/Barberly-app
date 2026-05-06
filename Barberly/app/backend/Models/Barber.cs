using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Barber
{
    [Key]
    public Guid barberId { get; set; } = Guid.NewGuid();
    public required string firstName { get; set; }
    public required string lastName { get; set; }
    public required string email { get; set; }
    public string? phoneNumber { get; set; }
}
