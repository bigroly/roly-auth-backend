using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SAM;
using Constructs;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace RolyAuth
{
    public class RolyAuthStack : Stack
    {
        internal RolyAuthStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string infraPrefix = "RolyApps-Auth";

            // Cognito user pool
            var userPool = new UserPool(this, $"{infraPrefix}-CognitoPool", new UserPoolProps
            {
                UserPoolName = "CognitoUserPool",
                RemovalPolicy = RemovalPolicy.DESTROY,
                SignInAliases = new SignInAliases
                {
                    Email = true
                },
                SelfSignUpEnabled = false
            });

            // Cognito user group
            var userPoolGroupReadUpdateAdd = new CfnUserPoolGroup(this, $"{infraPrefix}-UserPoolGroup", new CfnUserPoolGroupProps
            {
                UserPoolId = userPool.UserPoolId,
                Description = "Users of Roly Apps",
                GroupName = "rolyapps-users",
            });

            // Cognito App Client
            // todo - get this from SSM
            string[] CallbackUrls = new string[] { "http://localhost" };
            var cognitoAppClient = new UserPoolClient(this, "{infraPrefix}-CognitoClient", new UserPoolClientProps
            {
                UserPoolClientName = "RolyAppsCognitoClient",
                UserPool = userPool,
                AccessTokenValidity = Duration.Hours(1),
                IdTokenValidity = Duration.Minutes(20),
                RefreshTokenValidity = Duration.Hours(5),
                AuthFlows = new AuthFlow
                {
                    UserPassword = true,
                    UserSrp = true
                },
                OAuth = new OAuthSettings
                {
                    CallbackUrls = CallbackUrls,
                    Flows = new OAuthFlows
                    {
                        ImplicitCodeGrant = true
                    },
                    Scopes = new[] { OAuthScope.EMAIL, OAuthScope.OPENID }
                }
            });

            //var authLambdaFunc = new Function(this, $"{infraPrefix}-AuthLambdaFunc", new FunctionProps
            //{
            //    Runtime = Runtime.DOTNET_6,
            //    Handler = "AuthFunction::Lambda.AuthFunction.Function::FunctionHandler",
            //    Code = Code.FromAsset("/dist/AuthFunction"),
            //    Environment = new Dictionary<string, string>
            //    {
            //        {"REGION", this.Region},
            //        {"COGNITO_USER_POOL_ID", userPool.UserPoolId},
            //        {"CLIENT_ID", cognitoAppClient.UserPoolClientId}
            //    },
            //    Timeout = Duration.Minutes(1),
            //    MemorySize = 256
            //});

            // API Lambda definition
            var backendLambdaFunc = new Function(this, $"{infraPrefix}-BackendLambdaFunc", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Handler = "ApiFunction::Lambda.ApiFunction.Function::FunctionHandler",
                Code = Code.FromAsset("./src/dist/backendFunction.zip"),
                MemorySize = 1024
            });

            //var tokenAuthorizer = new TokenAuthorizer(this, "LambdaTokenAuthorizer", new TokenAuthorizerProps
            //{
            //    Handler = authLambdaFunc,
            //    IdentitySource = "method.request.header.authorization",
            //    ResultsCacheTtl = Duration.Seconds(0)
            //});

            // APIGateway 
            string apiName = $"{infraPrefix}-ApiGateway";
            var apiGateway = new RestApi(this, apiName, new RestApiProps()
            {
                RestApiName = apiName,
                Description = "Lambda Backend API",
                DeployOptions = new StageOptions
                {
                    StageName = "Prod",
                    ThrottlingBurstLimit = 10,
                    ThrottlingRateLimit = 10
                }
            });

            var authorizedMethodOptions = new MethodOptions
            {
                Authorizer = new CognitoUserPoolsAuthorizer(this, $"{infraPrefix}-CognitoPoolsAuthorizer", new CognitoUserPoolsAuthorizerProps { CognitoUserPools = new[] { userPool } }),
                AuthorizationType = AuthorizationType.COGNITO
            };

            // API Methods

            // Auth endpoints
            var authController = apiGateway.Root.AddResource("auth");
            var loginEndpoint = authController.AddResource("login");
            loginEndpoint.AddMethod("POST", new LambdaIntegration(backendLambdaFunc), new MethodOptions { AuthorizationType = AuthorizationType.NONE });

            var TestController = apiGateway.Root.AddResource("test");
            TestController.AddMethod("GET", new LambdaIntegration(backendLambdaFunc), authorizedMethodOptions);
        }
    }
}
