using ApiFunction.Enums;

namespace ApiFunction.Models.Auth.Request;

public record LoginGetOtpRequest
{
    public OtpMethod OtpMedium { get; set; }
    public string Target { get; set; }
}