using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.DTOsRequstsModels;

public class ResetPasswordDTOModel
{
    [Required]
    public string ResetPasswordToken { get; set; }

    [Required]
    public string NewPassword { get; set; }
}
