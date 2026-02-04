namespace Vital.Domain.Entities;

public class Receptionist
{
    public int ReceptionistId { get; set; }
    public int UserId { get; set; }
    public string Department { get; set; } = string.Empty;
    
    // Navigation properties
    public User User { get; set; } = null!;
}
