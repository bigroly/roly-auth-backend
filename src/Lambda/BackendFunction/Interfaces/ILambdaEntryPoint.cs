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
        Task<APIGatewayProxyResponse> RegisterOtpUser(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> ConfirmOtpUser(APIGatewayProxyRequest apiRequest);
        Task<APIGatewayProxyResponse> RegisterUser(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> InitiateOtpLogin(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> SubmitEmailOtp(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> LoginWithUsernamePassword(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> LoginWithRefreshToken(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> GetApps(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> BeginPasswordReset(APIGatewayProxyRequest request);
        Task<APIGatewayProxyResponse> ConfirmPasswordReset(APIGatewayProxyRequest request);
    }
}
