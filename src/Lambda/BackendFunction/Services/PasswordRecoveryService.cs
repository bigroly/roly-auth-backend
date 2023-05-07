using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using ApiFunction.Interfaces;
using ApiFunction.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ApiFunction.Services
{
    public class PasswordRecoveryService: IPasswordRecoveryService
    {
        private readonly IConfiguration _config;
        private readonly IAmazonCognitoIdentityProvider _cognitoIdp;
        private readonly ILogger<CognitoService> _logger;
        private readonly IUtilities _utils;

        public PasswordRecoveryService(IConfiguration config, IAmazonCognitoIdentityProvider cognitoIdp, ILogger<CognitoService> logger, IUtilities utils)
        {
            _config = config;
            _cognitoIdp = cognitoIdp;
            _logger = logger;
            _utils = utils;
        }

        public async Task<APIGatewayProxyResponse> BeginPasswordRecovery(APIGatewayProxyRequest apiRequest)
        {
            BeginPwResetRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<BeginPwResetRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                return _utils.BadRequest("Email address missing from request. Please provide this and try agin.");
            }

            ForgotPasswordRequest idpRequest = new ForgotPasswordRequest()
            {
                ClientId = _config.GetValue<string>("userPoolClientId"),
                Username = request.Email
            };

            try
            {
                var idpResponse = await _cognitoIdp.ForgotPasswordAsync(idpRequest);
                return _utils.Ok(null, null);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error calling Cognito ForgotPasswordAsync for user with email:[{request.Email}]");
                return _utils.ServerError("Sorry, there was an issue trying to begin resetting your password.");
            }           
        }

        //public async Task<APIGatewayProxyResponse> ConfirmAndResetPassword(APIGatewayProxyRequest apiRequest)
        //{

        //}
    }
}
