namespace ApiFunction.Models.Auth.Request
{
    public record LoginUsernamePasswordRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
