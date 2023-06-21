using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    [Required]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
public class LoginInput
{
    public string Email { get; set; }
    public string Password { get; set; }
}
