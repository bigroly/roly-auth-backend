using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using ApiFunction.Interfaces;
using ApiFunction.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Services
{
    public class AppsService: IAppsService
    {
        private readonly IUtilities _utils;
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<AppsService> _logger;

        private DynamoDBOperationConfig _tableConfig;
        private JwtSecurityTokenHandler _jwtReader;

        public AppsService(IConfiguration config, IUtilities utils, IDynamoDBContext dynamoDbContext, ILogger<AppsService> logger)
        {
            _utils = utils;
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;

            _tableConfig = new DynamoDBOperationConfig()
            {
                OverrideTableName = config.GetValue<string>("appsTableName")
            };
            
            _jwtReader = new JwtSecurityTokenHandler();
        }

        public async Task<APIGatewayProxyResponse> GetApplications(APIGatewayProxyRequest request)
        {
            var jwtEncoded = request.Headers.Where(h => h.Key == "Authorization").FirstOrDefault().Value.Split(" ")[1];
            var token = _jwtReader.ReadJwtToken(jwtEncoded);
            var userEmail = token.Claims.Where(c => c.Type == "email").FirstOrDefault()?.Value;

            var allowedApps = new List<ApplicationModel>();
            var allApps = await _dynamoDbContext.ScanAsync<ApplicationModel>(new List<ScanCondition>(), _tableConfig).GetRemainingAsync();

            foreach (var app in allApps)
            {
                if (string.IsNullOrEmpty(app.WhitelistUsers))
                {
                    allowedApps.Add(app);
                    continue;
                }
                
                if(!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(app.WhitelistUsers))
                {
                    var allowedUsers = app.WhitelistUsers.Split(',');
                    if (allowedUsers.Contains(userEmail))
                    {
                        allowedApps.Add(app);
                    }
                }
            }

            var appsResponse = new GetAppsResponse
            {
                Apps = allowedApps
            };

            return _utils.Ok(JsonConvert.SerializeObject(appsResponse), "application/json");
        }
    }
}
