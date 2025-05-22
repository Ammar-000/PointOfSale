using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.UserRequestsModels;

public class CheckLoginModel
{
    [Required]
    public string UserNameOrEmail { get; set; }

    [Required]
    public string Password { get; set; }
}
