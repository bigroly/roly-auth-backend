using Amazon.Lambda.APIGatewayEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Interfaces
{
    public interface IUtilities
    {
        APIGatewayProxyResponse BadRequest(string message);
        APIGatewayProxyResponse ServerError(string message);
        APIGatewayProxyResponse Ok(string? bodySerialized, string? contentType);
    }
}
