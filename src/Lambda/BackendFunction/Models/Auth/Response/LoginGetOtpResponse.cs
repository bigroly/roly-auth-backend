using ApiFunction.Enums;

namespace ApiFunction.Models.Auth.Response;

public record LoginGetOtpResponse
{
    public OtpMethod OtpMethod { get; init; }
    public string SessionToken { get; init; }
}