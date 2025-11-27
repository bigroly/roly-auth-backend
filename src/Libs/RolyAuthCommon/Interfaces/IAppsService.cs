using Amazon.Lambda.APIGatewayEvents;
using ApiFunction.Models.App;

namespace RolyAuthCommon.Interfaces
{
    public interface IAppsService
    {
        Task<List<ApplicationModel>> GetUserApps(string userEmail);
        Task<APIGatewayProxyResponse> GetApplications(APIGatewayProxyRequest request);
    }
}
