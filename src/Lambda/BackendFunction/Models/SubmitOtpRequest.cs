namespace ApiFunction.Models;

public record SubmitOtpRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
    public string SessionToken { get; set; }
}