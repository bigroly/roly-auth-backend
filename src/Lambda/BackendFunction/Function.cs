using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using System;
using ApiFunction.Interfaces;
using ApiFunction;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ApiFunction
{

    public class Function
    {
        private readonly ILambdaEntryPoint _lambdaEntryPoint;

        public Function()
        {
            var startup = new Startup();
            IServiceProvider provider = startup.ConfigureServices();
            _lambdaEntryPoint = provider.GetRequiredService<ILambdaEntryPoint>();
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
            {
                var response = await _lambdaEntryPoint.RegisterUser(request);
                return response;
            }

            if(request.Path == "/account/login" && request.HttpMethod.ToLower() == "post")
            {
                var response = await _lambdaEntryPoint.LoginWithUsernamePassword(request);
                return response;
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "Unknown endpoint or method",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
        }
    }
}