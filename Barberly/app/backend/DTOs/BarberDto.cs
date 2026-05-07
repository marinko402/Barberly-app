namespace backend.Dtos;

public class BarberDto
{
    public required string firstName { get; set; }
    public required string lastName { get; set; }
    public required string email { get; set; }
    public string? phoneNumber { get; set; }
}
