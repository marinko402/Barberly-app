using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Salon
{
    [Key]
    public Guid salonId { get; set; } = Guid.NewGuid();
    public required string name { get; set; }
    public List<Barber> barbers { get; set; } = new List<Barber>();
    public string? address { get; set; }
    public string? city { get; set; }
    public string? OwnerId { get; set; }
    public Barber? owner { get; set; }
}
