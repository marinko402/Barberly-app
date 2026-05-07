using backend.Models;

namespace backend.Dtos;

public class BookingDto
{
    public Guid timeslotId { get; set; }
    public required string customerFirstName { get; set; }
    public required string customerLastName { get; set; }
    public required string customerEmail { get; set; }
    public string? customerPhoneNumber { get; set; }
}
