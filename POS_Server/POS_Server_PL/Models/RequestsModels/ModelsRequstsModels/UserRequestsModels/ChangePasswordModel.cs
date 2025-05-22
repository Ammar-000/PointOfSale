using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.UserRequestsModels;

public class ChangePasswordModel
{
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; }

    [Required]
    public string CurrentPassword { get; set; }

    [Required]
    public string NewPassword { get; set; }

}
