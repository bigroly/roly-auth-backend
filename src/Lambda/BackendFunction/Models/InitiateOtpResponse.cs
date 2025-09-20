using ApiFunction.Enums;

namespace ApiFunction.Models;

public record InitiateOtpResponse
{
    public OtpMethod OtpMethod { get; init; }
    public string SessionToken { get; init; }
}