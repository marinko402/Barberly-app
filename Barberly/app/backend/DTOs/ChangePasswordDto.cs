public class ChangePasswordDto
{
    public required string UserId { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
