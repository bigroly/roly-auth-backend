namespace ApiFunction.Models.Auth.Request;

public record LoginGetOtpRequest
{
    public string Email { get; set; }
}