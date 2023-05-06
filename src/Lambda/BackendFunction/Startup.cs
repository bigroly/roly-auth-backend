using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleSystemsManagement;
using ApiFunction.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return provider;
        }

        private void ConfigureLoggingAndConfigurations(ServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);
        }

        private void ConfigureApplicationServices(ServiceCollection services)
        {
            AWSOptions awsOptions = Configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddSingleton<ILambdaEntryPoint, LambdaEntryPoint>();

            //aws services
            services.AddAWSService<IAmazonSimpleSystemsManagement>();
            services.AddAWSService<IAmazonDynamoDB>();

            // Our services
            //services.AddSingleton<ICtAlertsCheckerProcess, CtAlertsCheckerProcess>();
        }
    }
}
