namespace ApiFunction.Models;

public record OtpRegistrationRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
}