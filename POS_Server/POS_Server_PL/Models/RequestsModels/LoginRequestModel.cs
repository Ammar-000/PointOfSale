using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels;

public class LoginRequestModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}
