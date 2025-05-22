using POS_Domains.Models;
using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.UserRequestsModels;

public class AddUserModel
{
    [Required]
    public UserModel User { get; set; }

    [Required]
    public string Password { get; set; }
}
