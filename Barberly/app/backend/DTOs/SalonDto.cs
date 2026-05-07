using backend.Models;

namespace backend.Dtos;

public class SalonDto
{
    public required string name { get; set; }
    public List<Barber> barbers { get; set; } = new List<Barber>();
    public string? address { get; set; }
}
