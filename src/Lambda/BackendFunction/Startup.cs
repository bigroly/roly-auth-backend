using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using ApiFunction.Interfaces;
using ApiFunction.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ApiFunction
{
    public class Startup
    {
        private readonly IConfigurationRoot Configuration;

        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddSystemsManager("/rolyapps-auth")
                .AddEnvironmentVariables()
                .Build();
        }

        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            ConfigureLoggingAndConfigurations(services);
            ConfigureApplicationServices(services);
            IServiceProvider provider = services.BuildServiceProvider();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return provider;
        }

        private void ConfigureLoggingAndConfigurations(ServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddLogging(loggingBuilder => 
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
            });
        }

        private void ConfigureApplicationServices(ServiceCollection services)
        {
            AWSOptions awsOptions = Configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddSingleton<ILambdaEntryPoint, LambdaEntryPoint>();

            //aws services
            services.AddAWSService<IAmazonSimpleSystemsManagement>();
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonCognitoIdentityProvider>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

            // Our services
            services.AddSingleton<ICognitoService, CognitoService>();
            services.AddSingleton<IPasswordRecoveryService, PasswordRecoveryService>();
            services.AddSingleton<IAppsService, AppsService>();
        }
    }
}
