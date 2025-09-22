namespace ApiFunction.Models.Auth.Request;

public record LoginSubmitOtpRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
    public string SessionToken { get; set; }
}