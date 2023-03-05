using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using System.Diagnostics.Tracing;

namespace RolyAuth
{
    public class RolyAuthStack : Stack
    {
        internal RolyAuthStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string poolName = "rolyapps-auth-userpool";
            var pool = new UserPool(scope, $"{poolName}-id", new UserPoolProps()
            {
                UserPoolName = poolName,
                Mfa = Mfa.OPTIONAL,
                PasswordPolicy = new PasswordPolicy() { MinLength = 8, RequireDigits = true, RequireSymbols = true }
            });

            string poolClientName = "rolyapps-auth-client";
            pool.AddClient(poolClientName, new UserPoolClientOptions()
            {
                UserPoolClientName = poolClientName,
                AccessTokenValidity = Duration.Hours(1),
                RefreshTokenValidity = Duration.Hours(24)
            });

            var authLambdaApi = new Function(this, "rolyapps-auth-lambdaApi", new FunctionProps()
            {
                Code = Code.FromAsset("publishedDirectory"),
            });

            string apiName = "rolyapps-auth-apiGateway";
            var apiGateway = new RestApi(this, apiName, new RestApiProps()
            {
                RestApiName = apiName,
                Description = "API for app authentication, etc"
            });

            var apiIntegration = new LambdaIntegration(authLambdaApi, new LambdaIntegrationOptions()
            {
                
            });

            apiGateway.Root.AddMethod("GET", apiIntegration);
        }
    }
}
