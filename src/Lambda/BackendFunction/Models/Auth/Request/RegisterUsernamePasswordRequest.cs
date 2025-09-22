namespace ApiFunction.Models.Auth.Request
{
    public record RegisterUsernamePasswordRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
}
