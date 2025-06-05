using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SSM;
using Constructs;
using System.Collections.Generic;

namespace RolyAuth
{
    public class RolyAuthStack : Stack
    {
        internal RolyAuthStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string infraPrefix = "RolyApps-Auth";
            string infraSsmPrefix = "/rolyapps-auth";

            // Cognito user pool
            string poolName = $"{infraPrefix}-CognitoPool";
            var userPool = new UserPool(this, poolName, new UserPoolProps
            {
                UserPoolName = poolName,
                RemovalPolicy = RemovalPolicy.DESTROY,
                SignInAliases = new SignInAliases
                {
                    Email = true
                },
                SelfSignUpEnabled = false,
                AccountRecovery = AccountRecovery.EMAIL_ONLY,
                CustomAttributes = new Dictionary<string, ICustomAttribute>
                {
                    {
                        "name", new StringAttribute(new StringAttributeProps { Mutable = true })
                    }
                },
                UserVerification = new UserVerificationConfig { 
                    EmailSubject = "Your RolyApps Verification Code",
                    EmailStyle = VerificationEmailStyle.CODE,
                    EmailBody = "Your RolyApps verification code is {####}. Please enter this code on the website where prompted and do NOT share this code with anyone."
                }
            });

            // Cognito user group
            var userPoolGroupReadUpdateAdd = new CfnUserPoolGroup(this, $"{infraPrefix}-UserPoolGroup", new CfnUserPoolGroupProps
            {
                UserPoolId = userPool.UserPoolId,
                Description = "Users of Roly Apps",
                GroupName = "rolyapps-users"
            });

            // Cognito App Client
            // todo - get this from SSM?
            string[] callbackUrls = ["http://localhost"];
            var cognitoAppClient = new UserPoolClient(this, "{infraPrefix}-CognitoClient", new UserPoolClientProps
            {
                UserPoolClientName = "RolyAppsCognitoClient",
                UserPool = userPool,
                AccessTokenValidity = Duration.Hours(1),
                IdTokenValidity = Duration.Minutes(20),
                RefreshTokenValidity = Duration.Days(30),
                AuthFlows = new AuthFlow
                {
                    UserPassword = true,
                    UserSrp = true,
                    AdminUserPassword = true,
                },
                OAuth = new OAuthSettings
                {
                    CallbackUrls = callbackUrls,
                    Flows = new OAuthFlows
                    {
                        ImplicitCodeGrant = true
                    },
                    Scopes = new[] { OAuthScope.EMAIL, OAuthScope.OPENID }
                }
            });

            // API Lambda definition
            var backendLambdaFunc = new Function(this, $"{infraPrefix}-BackendLambdaFunc", new FunctionProps
            {
                Runtime = Runtime.DOTNET_8,
                Handler = "ApiFunction::Lambda.ApiFunction.Function::FunctionHandler",
                Code = Code.FromAsset("./src/dist/backendFunction.zip"),
                MemorySize = 1024
            });

            backendLambdaFunc.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps()
            {
                Actions = new[] {
                    "ssm:GetParameter",
                    "ssm:GetParametersByPath",
                    "dynamodb:DescribeTable",
                    "dynamodb:Query",
                    "dynamodb:Scan",
                    "cognito-idp:AdminCreateUser",
                    "cognito-idp:AdminEnableUser",
                    "cognito-idp:AdminSetUserPassword",
                    "cognito-idp:AdminInitiateAuth",
                    "cognito-idp:ConfirmForgotPassword"
                },
                Resources = new[] { "*" },
                Effect = Effect.ALLOW
            }));

            // API Gateway 
            string apiName = $"{infraPrefix}-ApiGateway";
            var apiGateway = new RestApi(this, apiName, new RestApiProps()
            {
                RestApiName = apiName,
                Description = "Auth Service Lambda Backend API",
                DeployOptions = new StageOptions
                {
                    StageName = "Prod",
                    ThrottlingBurstLimit = 10,
                    ThrottlingRateLimit = 10
                }
            });

            var corsAnyOrigin = new ResourceOptions()
            {
                DefaultCorsPreflightOptions = new CorsOptions
                {
                    AllowOrigins = new[] { "*" },
                    AllowHeaders = new[] { "Content-Type", "X-Amz-Date", "Authorization", "X-Api-Key", "X-Amz-Security-Token" }
                }
            };

            var corsLimitedOrigins = new ResourceOptions()
            {
                DefaultCorsPreflightOptions = new CorsOptions
                {
                    AllowOrigins = new[] { "https://auth.rolyapps.com", "http://localhost:4200" },
                    AllowHeaders = new[] { "Content-Type", "X-Amz-Date", "Authorization", "X-Api-Key", "X-Amz-Security-Token" }
                }
            };
            
            var cognitoAuthorizer = new CognitoUserPoolsAuthorizer(this, $"{infraPrefix}-CognitoPoolsAuthorizer", 
                new CognitoUserPoolsAuthorizerProps { 
                    CognitoUserPools = [userPool]
                });
            
            var authorizedMethodOptions = new MethodOptions
            {
                Authorizer = cognitoAuthorizer,
                AuthorizationType = AuthorizationType.COGNITO
            };

            // Auth endpoints
            var authController = apiGateway.Root.AddResource("account");
            
            var registerEndpoint = authController.AddResource("register", corsAnyOrigin);
            registerEndpoint.AddMethod("POST", new LambdaIntegration(backendLambdaFunc), new MethodOptions { AuthorizationType = AuthorizationType.NONE });

            var loginEndpoint = authController.AddResource("login", corsAnyOrigin);
            loginEndpoint.AddMethod("POST", new LambdaIntegration(backendLambdaFunc), new MethodOptions { AuthorizationType = AuthorizationType.NONE });
            var loginWithTokenEndpoint = loginEndpoint.AddResource("token", corsAnyOrigin);
            loginWithTokenEndpoint.AddMethod("POST", new LambdaIntegration(backendLambdaFunc), new MethodOptions { AuthorizationType = AuthorizationType.NONE });
            
            var beginPwResetEndpoint = authController.AddResource("forgotPassword", corsLimitedOrigins);
            beginPwResetEndpoint.AddMethod("POST", new LambdaIntegration(backendLambdaFunc), new MethodOptions { AuthorizationType = AuthorizationType.NONE });

            var confirmPwResetEndpoint = authController.AddResource("resetPassword", corsLimitedOrigins);
            confirmPwResetEndpoint.AddMethod("POST", new LambdaIntegration(backendLambdaFunc), new MethodOptions { AuthorizationType = AuthorizationType.NONE });

            // Apps endpoints
            var appsController = apiGateway.Root.AddResource("apps", corsLimitedOrigins);
            appsController.AddMethod("GET", new LambdaIntegration(backendLambdaFunc), authorizedMethodOptions);

            // Apps table
            var appsTableName = "rolyauth-apps";
            var appsTable = new Table(this, appsTableName, new TableProps
            {
                TableName = appsTableName,
                PartitionKey = new Attribute
                {
                    Name = "AppName",
                    Type = AttributeType.STRING
                },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            });            

            // Output information to SSM
            var userPoolIdSsm = new StringParameter(this, $"{infraPrefix}-userPoolIdSsm-ssm", new StringParameterProps()
            {
                Description = "Cognito User Pool Id",
                ParameterName = $"{infraSsmPrefix}/cognitoPoolId",
                StringValue = userPool.UserPoolId
            });

            var userPoolClientNameSsm = new StringParameter(this, $"{infraPrefix}-userPoolClientName-ssm", new StringParameterProps()
            {
                Description = "Cognito User Pool Client Name",
                ParameterName = $"{infraSsmPrefix}/userPoolClientName",
                StringValue = cognitoAppClient.UserPoolClientName
            });

            var userPoolClientIdSsm = new StringParameter(this, $"{infraPrefix}-userPoolClientId-ssm", new StringParameterProps()
            {
                Description = "Cognito User Pool Client Id",
                ParameterName = $"{infraSsmPrefix}/userPoolClientId",
                StringValue = cognitoAppClient.UserPoolClientId
            });

            var apiGatewayUrlSsm = new StringParameter(this, $"{infraPrefix}-apiGatewayUrl-ssm", new StringParameterProps()
            {
                Description = "Auth API Gateway Url",
                ParameterName = $"{infraSsmPrefix}/apiGatewayUrl",
                StringValue = apiGateway.Url
            });

            var appsTableNameSsm = new StringParameter(this, $"{infraPrefix}-appsTableName-ssm", new StringParameterProps()
            {
                Description = "Apps Table Name",
                ParameterName = $"{infraSsmPrefix}/appsTableName",
                StringValue = appsTable.TableName
            });
        }
    }
}
