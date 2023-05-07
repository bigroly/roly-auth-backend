using Amazon.Lambda.APIGatewayEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Interfaces
{
    public interface IPasswordRecoveryService
    {
        Task<APIGatewayProxyResponse> BeginPasswordRecovery(APIGatewayProxyRequest apiRequest);
        Task<APIGatewayProxyResponse> ConfirmAndResetPassword(APIGatewayProxyRequest apiRequest);
    }
}
