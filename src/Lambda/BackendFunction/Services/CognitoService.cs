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
using ApiFunction.Enums;
using ApiFunction.Util;

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

        public async Task<APIGatewayProxyResponse> RegisterOtpUser(APIGatewayProxyRequest apiRequest)
        {
            OtpRegistrationRequest requestBody;
            try
            {
                requestBody = JsonConvert.DeserializeObject<OtpRegistrationRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }
            
            if (string.IsNullOrEmpty(requestBody.Email) || string.IsNullOrEmpty(requestBody.Name))
            {
                return _utils.BadRequest("Email or Name missing in request. Please check parameters and try again.");
            }
            
            AdminCreateUserRequest createUserReq = new AdminCreateUserRequest()
            {
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                Username = requestBody.Email,
                UserAttributes = new List<AttributeType>()
                {
                    new()
                    {
                        Name = "email",
                        Value = requestBody.Email
                    },
                    new()
                    {
                        Name = "custom:name",
                        Value = requestBody.Name
                    },
                    new()
                    {
                        Name = "email_verified",
                        Value = "False"
                    },
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
            
            _logger.LogInformation("User created successfully: {user}. User status: {status}", userCreated.User.Username, userCreated.User.UserStatus.ToString());
            var authReq = new AdminInitiateAuthRequest
            {
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                ClientId = _config.GetValue<string>("userPoolClientId"),
                AuthFlow = AuthFlowType.USER_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", requestBody.Email },
                    { "PREFERRED_CHALLENGE", "EMAIL_OTP"}
                }
            };

            return await InitiateCognitoAuth(authReq);
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

            if (!PasswordUtil.Validate(requestBody.Password))
            {
                return _utils.BadRequest("Invalid password. Please check it meets minimum requirements (at last 6 characters long, contains number, uppercase, lowercase and special character) and try again.");
            }

            AdminCreateUserRequest createUserReq = new AdminCreateUserRequest()
            {
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                Username = requestBody.Email,
                // Generate a "different" password to the one that will be set a couple lines down
                TemporaryPassword = PasswordUtil.GenerateRandomPassword(),
                UserAttributes = new List<AttributeType>()
                {
                    new()
                    {
                        Name = "email",
                        Value = requestBody.Email
                    },
                    // todo - at some point would be cool to explore email verification
                    new()
                    {
                        Name = "email_verified",
                        Value = "True"
                    },
                    new()
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

        public async Task<APIGatewayProxyResponse> InitiateOtpLogin(APIGatewayProxyRequest apiRequest)
        {
            EmailOtpRequest requestBody;
            try
            {
                requestBody = JsonConvert.DeserializeObject<EmailOtpRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }

            if (string.IsNullOrEmpty(requestBody?.Email))
            {
                return _utils.BadRequest("Email missing in request. Please check parameters and try again.");
            }
            
            var authReq = new AdminInitiateAuthRequest
            {
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                ClientId = _config.GetValue<string>("userPoolClientId"),
                AuthFlow = AuthFlowType.USER_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", requestBody.Email },
                    { "PREFERRED_CHALLENGE", "EMAIL_OTP"}
                }
            };

            return await InitiateCognitoAuth(authReq);
        }

        public async Task<APIGatewayProxyResponse> SubmitEmailOtp(APIGatewayProxyRequest apiRequest)
        {
            SubmitOtpRequest requestBody;
            try
            {
                requestBody = JsonConvert.DeserializeObject<SubmitOtpRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }

            if (string.IsNullOrEmpty(requestBody.Code))
            {
                return _utils.BadRequest("OTP missing in request. Please check parameters and try again.");
            }

            var cognitoRequest = new AdminRespondToAuthChallengeRequest()
            {
                ChallengeName = ChallengeNameType.EMAIL_OTP,
                Session = requestBody.SessionToken,
                ChallengeResponses = new Dictionary<string, string>()
                {
                    { "USERNAME" , requestBody.Email },
                    { "EMAIL_OTP_CODE", requestBody.Code }
                },
                ClientId = _config.GetValue<string>("userPoolClientId"),
                UserPoolId = _config.GetValue<string>("cognitoPoolId")
            };

            try
            {
                var clientResponse = await _cognitoIdp.AdminRespondToAuthChallengeAsync(cognitoRequest);

                // Return tokens for Password Login:
                var apiResponse = new LoginResponse
                {
                    IdToken = clientResponse.AuthenticationResult.IdToken,
                    AccessToken = clientResponse.AuthenticationResult.AccessToken,
                    Expiry = clientResponse.AuthenticationResult.ExpiresIn ?? (long)0,
                    RefreshToken = clientResponse.AuthenticationResult.RefreshToken
                };
                return _utils.Ok(JsonConvert.SerializeObject(apiResponse), "application/json");
            }
            catch (UserNotFoundException e)
            {
                return _utils.BadRequest("Sorry, your username or password is incorrect.");
            }
            catch (CodeMismatchException e)
            {
                return _utils.BadRequest("The provided code is incorrect. Please try again.");
            }
            catch (ExpiredCodeException e)
            {
                return _utils.BadRequest("The provided code has expired. Please request a new one and try again.");
            }
            catch (NotAuthorizedException e)
            {
                return _utils.BadRequest("Sorry, your provided details are incorrect. Please try again.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception encountered in POST to /api/auth/login");
                return _utils.ServerError("Sorry, something went wrong logging you in. We'll look into it.");
            }
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

            return await InitiateCognitoAuth(authReq);
        }

        public async Task<APIGatewayProxyResponse> LoginWithRefreshToken(APIGatewayProxyRequest apiRequest)
        {
            LoginWithTokenRequest requestBody;
            try
            {
                requestBody = JsonConvert.DeserializeObject<LoginWithTokenRequest>(apiRequest.Body);
            }
            catch (Exception ex)
            {
                return _utils.BadRequest("Sorry, there was a problem validating the request. Please check parameters and try again.");
            }

            if (string.IsNullOrEmpty(requestBody.RefreshToken))
            {
                return _utils.BadRequest("Refresh Token missing in request. Please check parameters and try again.");
            }

            var authReq = new AdminInitiateAuthRequest
            {
                UserPoolId = _config.GetValue<string>("cognitoPoolId"),
                ClientId = _config.GetValue<string>("userPoolClientId"),
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "REFRESH_TOKEN", requestBody.RefreshToken }
                }
            };

            return await InitiateCognitoAuth(authReq);
        }

        private async Task<APIGatewayProxyResponse> InitiateCognitoAuth(AdminInitiateAuthRequest request)
        {
            try
            {
                var clientResponse = await _cognitoIdp.AdminInitiateAuthAsync(request);

                // OTP login initaited - return different model:
                if (request.AuthParameters.Any(a => a.Key == "PREFERRED_CHALLENGE"))
                {
                    var otpResponse = new InitiateOtpResponse()
                    {
                        OtpMethod = OtpMethod.Email,
                        SessionToken = clientResponse.Session
                    };
                    return _utils.Ok(JsonConvert.SerializeObject(otpResponse), "application/json");   
                }
                
                // Return tokens for Password Login:
                var apiResponse = new LoginResponse
                {
                    IdToken = clientResponse.AuthenticationResult.IdToken,
                    AccessToken = clientResponse.AuthenticationResult.AccessToken,
                    Expiry = clientResponse.AuthenticationResult.ExpiresIn ?? (long)0,
                    RefreshToken = clientResponse.AuthenticationResult.RefreshToken
                };
                return _utils.Ok(JsonConvert.SerializeObject(apiResponse), "application/json");
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
    }
}
