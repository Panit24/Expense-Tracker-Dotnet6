using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();  // Auto-generate GUID

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Phone]
    public string PhoneNumber { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    public DateTime? UpdatedAt { get; set; }
}