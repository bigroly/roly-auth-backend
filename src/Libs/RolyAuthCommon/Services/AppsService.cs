using System.IdentityModel.Tokens.Jwt;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using ApiFunction.Models.App;
using ApiFunction.Models.App.Response;
using ApiFunction.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RolyAuthCommon.Interfaces;

namespace RolyAuthCommon.Services
{
    public class AppsService: IAppsService
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<AppsService> _logger;

        private DynamoDBOperationConfig _tableConfig;
        private JwtSecurityTokenHandler _jwtReader;

        public AppsService(IConfiguration config, IDynamoDBContext dynamoDbContext, ILogger<AppsService> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;

            _tableConfig = new DynamoDBOperationConfig()
            {
                OverrideTableName = config["appsTableName"]
            };
            
            _jwtReader = new JwtSecurityTokenHandler();
        }

        public async Task<List<ApplicationModel>> GetUserApps(string userEmail)
        {
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
            
            return allowedApps;
        }

        public async Task<APIGatewayProxyResponse> GetApplications(APIGatewayProxyRequest request)
        {
            var jwtEncoded = request.Headers.FirstOrDefault(h => h.Key == "Authorization").Value.Split(" ")[1];
            var token = _jwtReader.ReadJwtToken(jwtEncoded);
            var userEmail = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            var allowedApps = await GetUserApps(userEmail);

            var appsResponse = new GetAppsResponse
            {
                Apps = allowedApps.OrderBy(a => a.AppName).ToList(),
            };

            return ApiGatewayUtil.Ok(JsonConvert.SerializeObject(appsResponse), "application/json");
        }
    }
}
