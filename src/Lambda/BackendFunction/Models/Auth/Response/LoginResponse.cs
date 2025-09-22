namespace ApiFunction.Models.Auth.Response
{
    public record LoginResponse
    {
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public long Expiry { get ; set; }
        public string RefreshToken { get; set; }
    }
}
