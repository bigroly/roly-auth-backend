namespace ApiFunction.Models.Auth.Request;

public record RegisterOtpRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
}