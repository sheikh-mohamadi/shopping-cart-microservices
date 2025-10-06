using System.ComponentModel.DataAnnotations;

namespace Cart.Domain.Models;

public class User
{
    public Guid Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
}