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
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            switch (request.Path)
            {
                default:
                    Console.WriteLine(request.Path);
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = "Unknown endpoint",
                        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                    };
            }
            

            //switch (request.HttpMethod.ToUpper())
            //{
            //  case "GET":
            //    return new APIGatewayProxyResponse
            //    {
            //      StatusCode = (int)HttpStatusCode.OK,
            //      Body = "Hello",
            //      Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            //    };
            //  case "POST":
            //    return new APIGatewayProxyResponse
            //    {
            //      StatusCode = (int)HttpStatusCode.Created,
            //      Body = "Created",
            //      Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            //    };
            //  default:
            //    return new APIGatewayProxyResponse
            //    {
            //      StatusCode = (int)HttpStatusCode.BadRequest,
            //      Body = "Invalid HttpMethod",
            //      Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            //    };
            //}
        }
    }
}