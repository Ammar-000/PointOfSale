using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.DTOsRequstsModels;

public class ChangePasswordDTOModel
{
    [Required]
    public string CurrentPassword { get; set; }

    [Required]
    public string NewPassword { get; set; }

}
