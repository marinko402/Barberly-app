using backend.Models;

namespace backend.Dtos;

public class TimeslotDto
{
    public DateOnly date { get; set; }
    public TimeOnly startTime { get; set; }
    public int duration { get; set; }

    public Guid salonId { get; set; }
    public Guid barberId { get; set; }
    public bool isBooked { get; set; }
}
