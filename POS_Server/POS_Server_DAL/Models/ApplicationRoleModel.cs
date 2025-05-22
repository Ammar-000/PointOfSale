using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace POS_Server_DAL.Models;

public class ApplicationRoleModel : IdentityRole
{
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [MaxLength(450)]
    public string CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }

}