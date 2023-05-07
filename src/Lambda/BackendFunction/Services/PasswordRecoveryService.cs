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

            try
            {
                ForgotPasswordRequest idpRequest = new ForgotPasswordRequest()
                {
                    ClientId = _config.GetValue<string>("userPoolClientId"),
                    Username = request.Email
                };

                var idpResponse = await _cognitoIdp.ForgotPasswordAsync(idpRequest);
                return _utils.Ok(null, null);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error calling Cognito ForgotPasswordAsync for user with email:[{request.Email}]");
                return _utils.ServerError("Sorry, there was an issue trying to begin resetting your password.");
            }           
        }

        public async Task<APIGatewayProxyResponse> ConfirmAndResetPassword(APIGatewayProxyRequest apiRequest)
        {
            ConfirmPasswordResetRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<ConfirmPasswordResetRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.ConfirmationCode) || string.IsNullOrEmpty(request.NewPassword))
            {
                return _utils.BadRequest("Email, Confirmation Code or New Password missing from request. Please provide this and try agin.");
            }

            try
            {
                ConfirmForgotPasswordRequest idpRequest = new ConfirmForgotPasswordRequest
                {
                    ClientId = _config.GetValue<string>("userPoolClientId"),
                    Username = request.Email,
                    Password = request.NewPassword,
                    ConfirmationCode = request.ConfirmationCode
                };

                var idpResponse = await _cognitoIdp.ConfirmForgotPasswordAsync(idpRequest);

                if(idpResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    return _utils.BadRequest("There was an issue with the provided password or reset code. Please check these and try again");
                }

                return _utils.Ok(null, null);               
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error calling Cognito ConfirmForgotPasswordAsync for user with email:[{request.Email}]");
                return _utils.BadRequest(ex.Message);
            }
        }
    }
}
