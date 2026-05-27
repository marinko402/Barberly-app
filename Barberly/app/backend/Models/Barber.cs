using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace backend.Models;

public class Barber : IdentityUser
{
    public required string firstName { get; set; }
    public required string lastName { get; set; }
    public DateOnly BirthDate { get; set; }
    public Guid? SalonId { get; set; }
    public Salon? salon { get; set; }
}
