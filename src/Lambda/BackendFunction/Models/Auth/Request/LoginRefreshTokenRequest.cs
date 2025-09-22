namespace ApiFunction.Models.Auth.Request
{
    public record LoginRefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
