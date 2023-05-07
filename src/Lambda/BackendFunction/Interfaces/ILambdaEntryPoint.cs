using Amazon.Lambda.APIGatewayEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Interfaces
{
    public interface ILambdaEntryPoint
    {
        Task<APIGatewayProxyResponse> RegisterUser(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> LoginWithUsernamePassword(APIGatewayProxyRequest request);
        APIGatewayProxyResponse GetApps();
        Task<APIGatewayProxyResponse> BeginPasswordReset(APIGatewayProxyRequest request);
    }
}
