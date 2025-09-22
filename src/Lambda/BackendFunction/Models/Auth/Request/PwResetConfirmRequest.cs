namespace ApiFunction.Models.Auth.Request
{
    public record PwResetConfirmRequest
    {
        public string Email { get; set; }
        public string ConfirmationCode { get; set; }
        public string NewPassword { get; set; }
    }
}
