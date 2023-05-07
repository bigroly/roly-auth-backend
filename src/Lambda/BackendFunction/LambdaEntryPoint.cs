using Amazon.Lambda.APIGatewayEvents;
using ApiFunction.Interfaces;
using ApiFunction.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction
{
    public class LambdaEntryPoint: ILambdaEntryPoint
    {
        private readonly ICognitoService _cognitoService;
        private readonly IAppsService _appsService;

        public LambdaEntryPoint(ICognitoService cognitoService, IAppsService appsService)
        {
            _cognitoService = cognitoService;
            _appsService = appsService;
        }

        public async Task<APIGatewayProxyResponse> RegisterUser(APIGatewayProxyRequest request)
        {
            return await _cognitoService.RegisterUser(request);
        }

        public async Task<APIGatewayProxyResponse> LoginWithUsernamePassword(APIGatewayProxyRequest request)
        {
            return await _cognitoService.LoginWithUsernamePassword(request);
        }

        public APIGatewayProxyResponse GetApps()
        {
            return _appsService.GetApplications();
        }
    }
}
