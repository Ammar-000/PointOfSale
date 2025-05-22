using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.ModelsRequstsModels;

public class ModifyUserRolesModel
{
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; }

    [Required]
    [MaxLength(450)]
    public string RoleId { get; set; }

}
