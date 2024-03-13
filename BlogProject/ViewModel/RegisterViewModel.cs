using System.ComponentModel.DataAnnotations;

namespace BlogProject.ViewModel;

public class RegisterViewModel
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage ="Hasła nie pasują")]
    public string ConfirmPassword { get; set; }
}
