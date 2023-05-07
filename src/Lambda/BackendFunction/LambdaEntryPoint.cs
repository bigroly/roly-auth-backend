﻿using Amazon.Lambda.APIGatewayEvents;
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

        public async Task<APIGatewayProxyResponse> LoginWithUsernamePassword(APIGatewayProxyRequest request)
        {
            return await _cognitoService.LoginWithUsernamePassword(request);
        }

        public APIGatewayProxyResponse GetApps()
        {
            return _appsService.GetApplications();
        }

        public async Task<APIGatewayProxyResponse> BeginPasswordReset(APIGatewayProxyRequest request)
        {
            return await _passwordRecoveryService.BeginPasswordRecovery(request);
        }
    }
}
