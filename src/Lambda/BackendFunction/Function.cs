using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using System;
using ApiFunction.Interfaces;
using ApiFunction;
using Microsoft.Extensions.DependencyInjection;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ApiFunction
{

    public class Function
    {
        private readonly ILambdaEntryPoint _lambdaEntryPoint;
        private readonly ILogger<Function> _logger;

        public Function()
        {
            var startup = new Startup();
            IServiceProvider provider = startup.ConfigureServices();
            _lambdaEntryPoint = provider.GetRequiredService<ILambdaEntryPoint>();
            _logger = provider.GetRequiredService<ILogger<Function>>();
        }

        /// <summary>
        /// Lambda function handler for API requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>APIGatewayProxyResponse</returns>
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if(request.Path == "/account/register" && request.HttpMethod.ToLower() == "post")
                return await _lambdaEntryPoint.RegisterUser(request);

            if (request.Path == "/account/login" && request.HttpMethod.ToLower() == "post")
                return await _lambdaEntryPoint.LoginWithUsernamePassword(request);
            
            if (request.Path == "/account/login/requestOtp" && request.HttpMethod.ToLower() == "post")
                return await _lambdaEntryPoint.InitiateOtpLogin(request);
            
            if (request.Path == "/account/login/submitEmailOtp" && request.HttpMethod.ToLower() == "post")
                return await _lambdaEntryPoint.SubmitEmailOtp(request);

            if (request.Path == "/account/login/token" && request.HttpMethod.ToLower() == "post")
                return await _lambdaEntryPoint.LoginWithRefreshToken(request);

            if (request.Path == "/account/forgotPassword" && request.HttpMethod.ToLower() == "post")
                return await _lambdaEntryPoint.BeginPasswordReset(request);

            if (request.Path == "/account/resetPassword" && request.HttpMethod.ToLower() == "post")
                return await _lambdaEntryPoint.ConfirmPasswordReset(request);

            if (request.Path == "/apps" && request.HttpMethod.ToLower() == "get")
                return await _lambdaEntryPoint.GetApps(request);


            _logger.LogError($"Received request for unknown resource Path:[{request.Path}], Method:[{request.HttpMethod}]");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "Unknown endpoint or method",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
        }
    }
}