using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Amazon.Runtime.Internal.Util;
using ApiFunction.Interfaces;
using ApiFunction.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiFunction.Services
{
    public class CognitoService: ICognitoService
    {
        private readonly IConfiguration _config;
        private readonly IAmazonCognitoIdentityProvider _cognitoIdp;
        private readonly ILogger<CognitoService> _logger;
        private readonly IUtilities _utils;

        public CognitoService(IConfiguration config, IAmazonCognitoIdentityProvider cognitoIdp, ILogger<CognitoService> logger, IUtilities utils)
        {
            _config = config;
            _cognitoIdp = cognitoIdp;
            _logger = logger;
            _utils = utils;
        }

        public async Task<APIGatewayProxyResponse> RegisterUser(APIGatewayProxyRequest apiRequest)
        {
            RegistrationRequest requestBody;
            try
            {
                requestBody = JsonConvert.DeserializeObject<RegistrationRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }

            if (string.IsNullOrEmpty(requestBody.Email) || string.IsNullOrEmpty(requestBody.Password))
            {
                return _utils.BadRequest("Username or Password missing in request. Please check parameters and try again.");
            }

            if (!PasswordValid(requestBody.Password))
            {
                return _utils.BadRequest("Invalid password. Please check it meets minimum requirements (at last 6 characters long, contains number, uppercase, lowercase and special character) and try again.");
            }

            AdminCreateUserRequest createUserReq = new AdminCreateUserRequest()
            {
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                Username = requestBody.Email,
                // dumb hack to generate a "different" password to the one that will be set a couple lines down
                TemporaryPassword = $"{requestBody.Password}&1",
                UserAttributes = new List<AttributeType>()
                {
                    new AttributeType
                    {
                        Name = "email",
                        Value = requestBody.Email
                    },
                    // todo - at some point would be cool to explore email verification
                    new AttributeType
                    {
                        Name = "email_verified",
                        Value = "True"
                    },
                    new AttributeType
                    {
                        Name = "custom:name",
                        Value = requestBody.Name
                    }
                },
                DesiredDeliveryMediums = new List<string>() { "EMAIL" },
                MessageAction = "SUPPRESS"
            };

            AdminCreateUserResponse userCreated;
            try
            {
                userCreated = await _cognitoIdp.AdminCreateUserAsync(createUserReq);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating User with email: [{requestBody.Email}] - {ex.Message}", ex);
                return _utils.ServerError("Sorry, something went wrong creating your account. We'll look into it.");
            }

            AdminSetUserPasswordRequest confirmPw = new AdminSetUserPasswordRequest()
            {
                Username = requestBody.Email,
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                Password = requestBody.Password,
                Permanent = true
            };

            AdminSetUserPasswordResponse setUserPw;
            try
            {
                setUserPw = await _cognitoIdp.AdminSetUserPasswordAsync(confirmPw);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error setting pw for User with email: [{requestBody.Email}]");
                return _utils.ServerError("Sorry, something went wrong creating your account. We'll look into it.");
            }

            return _utils.Ok(null, null);
        }

        public async Task<APIGatewayProxyResponse> LoginWithUsernamePassword(APIGatewayProxyRequest apiRequest)
        {
            LoginRequest requestBody;
            try
            {
                requestBody = JsonConvert.DeserializeObject<LoginRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }

            if (string.IsNullOrEmpty(requestBody.Email) || string.IsNullOrEmpty(requestBody.Password))
            {
                return _utils.BadRequest("Username or Password missing in request. Please check parameters and try again.");
            }

            var authReq = new AdminInitiateAuthRequest
            {
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                ClientId = _config.GetValue<string>("userPoolClientId"),
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", requestBody.Email },
                    { "PASSWORD", requestBody.Password }
                }
            };

            try
            {
                var idpResponse = await _cognitoIdp.AdminInitiateAuthAsync(authReq);
                var clientResponse = new LoginResponse
                {
                    BearerToken = idpResponse.AuthenticationResult.IdToken,
                    Expiry = idpResponse.AuthenticationResult.ExpiresIn,
                    RefreshToken = idpResponse.AuthenticationResult.RefreshToken
                };
                return _utils.Ok(JsonConvert.SerializeObject(clientResponse), "application/json");
            }
            catch (UserNotFoundException e)
            {
                return _utils.BadRequest("Sorry, your username or password is incorrect.");
            }
            catch (PasswordResetRequiredException e)
            {
                return _utils.BadRequest("Sorry, this account is currently locked and requires a password reset.");
            }
            catch (NotAuthorizedException e)
            {
                return _utils.BadRequest("Sorry, your username or password is incorrect.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception encountered in POST to /api/auth/login");
                return _utils.ServerError("Sorry, something went wrong logging you in. We'll look into it.");
            }

        }

        private bool PasswordValid(string password)
        {
            if (password.Length < 6)
                return false;

            if (!Regex.IsMatch(password, @"[$^*.\[\]{}()?\-""!@#%&/\\,><':;|_~`=+]+"))
                return false;

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            if (!Regex.IsMatch(password, @"\d"))
                return false;

            return true;

        }
    }
}
