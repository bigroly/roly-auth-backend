namespace ApiFunction.Models.Auth.Request
{
    public record PwResetBeginRequest
    {
        public string Email { get; set; }
    }
}
