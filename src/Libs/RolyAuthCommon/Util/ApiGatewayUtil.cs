using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace ApiFunction.Util;

public static class ApiGatewayUtil
{
    public static bool TryParseRequest<T>(APIGatewayProxyRequest apiRequest, out T requestBody, out APIGatewayProxyResponse errorResponse)
    {
        requestBody = default;
        errorResponse = null;

        if (string.IsNullOrEmpty(apiRequest.Body))
        {
            errorResponse = BadRequest("Request body is empty. Please check parameters and try again.");
            return false;
        }
            
        try
        {
            requestBody = JsonConvert.DeserializeObject<T>(apiRequest.Body);
            if (requestBody == null)
            {
                errorResponse = BadRequest("Sorry, there was a problem with the request body. Please check parameters and try again.");
                return false;
            }
        }
        catch (JsonException)
        {
            errorResponse = BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            return false;
        }

        return true;
    }

    public static APIGatewayProxyResponse BadRequest(string message)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = message,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static APIGatewayProxyResponse ServerError(string message)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Body = message,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static APIGatewayProxyResponse Ok(string? bodySerialized, string? contentType) {
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