namespace ApiFunction.Models.App.Response
{
    public record GetAppsResponse
    {
        public List<ApplicationModel> Apps { get; set; }
    }
}
