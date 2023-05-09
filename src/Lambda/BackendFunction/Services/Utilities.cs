using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime.Internal.Transform;
using ApiFunction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Services
{
    public class Utilities: IUtilities
    {
        public Utilities() { }

        public APIGatewayProxyResponse BadRequest(string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = message,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public APIGatewayProxyResponse ServerError(string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = message,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        public APIGatewayProxyResponse Ok(string? bodySerialized, string? contentType) {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = bodySerialized,
                Headers = new Dictionary<string, string> { 
                    { "Content-Type", string.IsNullOrEmpty(contentType) ? "text/plain" : contentType },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "*" }
                }
            };
        }
    }
}
