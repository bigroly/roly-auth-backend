using Amazon.Lambda.APIGatewayEvents;
using ApiFunction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Interfaces
{
    public interface ICognitoService
    {
        Task<APIGatewayProxyResponse> RegisterUser(APIGatewayProxyRequest apiRequest);
        Task<APIGatewayProxyResponse> InitiateOtpLogin(APIGatewayProxyRequest apiRequest);
        Task<APIGatewayProxyResponse> LoginWithUsernamePassword(APIGatewayProxyRequest apiRequest);
        Task<APIGatewayProxyResponse> LoginWithRefreshToken(APIGatewayProxyRequest apiRequest);
    }
}
