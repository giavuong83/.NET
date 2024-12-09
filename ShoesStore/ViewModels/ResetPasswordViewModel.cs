using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; }
} 