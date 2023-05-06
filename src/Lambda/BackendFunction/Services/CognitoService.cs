using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime.Internal;
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

            if (string.IsNullOrEmpty(requestBody.Username) || string.IsNullOrEmpty(requestBody.Password))
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
                Username = requestBody.Username,
                // dumb hack to generate a "different" password to the one that will be set a couple lines down
                TemporaryPassword = $"{requestBody.Password}&1",
                UserAttributes = new List<AttributeType>()
                {
                    new AttributeType
                    {
                        Name = "email",
                        Value = requestBody.Username
                    },
                    // todo - at some point would be cool to explore email verification
                    new AttributeType
                    {
                        Name = "email_verified",
                        Value = "True"
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
                _logger.LogError($"Error creating User with username: [{requestBody.Username}] - {ex.Message}", ex);
                return _utils.ServerError("Sorry, something went wrong creating your account. We'll look into it.");
            }

            AdminSetUserPasswordRequest confirmPw = new AdminSetUserPasswordRequest()
            {
                Username = requestBody.Username,
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
                _logger.LogError($"Error setting pw for User with username: [{requestBody.Username}]", ex);
                return _utils.ServerError("Sorry, something went wrong creating your account. We'll look into it.");
            }

            return _utils.Ok(null, null);
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
