using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Booking
{
    [Key]
    public Guid bookingId { get; set; } = Guid.NewGuid();
    public Timeslot? timeslot { get; set; }
    public required string customerFirstName { get; set; }
    public required string customerLastName { get; set; }
    public required string customerEmail { get; set; }
    public string? customerPhoneNumber { get; set; }
}
