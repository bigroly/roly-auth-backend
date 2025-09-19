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
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        public LambdaEntryPoint(ICognitoService cognitoService, IAppsService appsService, IPasswordRecoveryService passwordRecoveryService)
        {
            _cognitoService = cognitoService;
            _appsService = appsService;
            _passwordRecoveryService = passwordRecoveryService;
        }

        public async Task<APIGatewayProxyResponse> RegisterUser(APIGatewayProxyRequest request)
        {
            return await _cognitoService.RegisterUser(request);
        }

        public async Task<APIGatewayProxyResponse> InitiateOtpLogin(APIGatewayProxyRequest request)
        {
            return await _cognitoService.InitiateOtpLogin(request);       
        }

        public async Task<APIGatewayProxyResponse> LoginWithUsernamePassword(APIGatewayProxyRequest request)
        {
            return await _cognitoService.LoginWithUsernamePassword(request);
        }

        public async Task<APIGatewayProxyResponse> LoginWithRefreshToken(APIGatewayProxyRequest request)
        {
            return await _cognitoService.LoginWithRefreshToken(request);
        }

        public async Task<APIGatewayProxyResponse> GetApps(APIGatewayProxyRequest request)
        {
            return await _appsService.GetApplications(request);
        }

        public async Task<APIGatewayProxyResponse> BeginPasswordReset(APIGatewayProxyRequest request)
        {
            return await _passwordRecoveryService.BeginPasswordRecovery(request);
        }

        public async Task<APIGatewayProxyResponse> ConfirmPasswordReset(APIGatewayProxyRequest request)
        {
            return await _passwordRecoveryService.ConfirmAndResetPassword(request);
        }
    }
}
