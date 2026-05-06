using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Timeslot
{
    [Key]
    public Guid timeslotId { get; set; } = Guid.NewGuid();
    public DateOnly date { get; set; }
    public TimeOnly startTime { get; set; }
    public int duration { get; set; }

    public Salon? salon { get; set; }
    public Barber? barber { get; set; }
}
