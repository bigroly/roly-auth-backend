using Amazon.Lambda.APIGatewayEvents;
using ApiFunction.Interfaces;
using ApiFunction.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Services
{
    public class AppsService: IAppsService
    {
        private readonly IUtilities _utils;

        public AppsService(IUtilities utils)
        {
            _utils = utils;
        }

        public APIGatewayProxyResponse GetApplications()
        {
            var appsResponse = new GetAppsResponse
            {
                Apps = new List<ApplicationModel>
                {
                    new ApplicationModel
                    {
                        Name = "MantainM8",
                        Description = "An application to help you keep track of and share your vehicle's maintenance history.",
                        LoginUrl = "https://google.com"
                    }
                }
            };

            return _utils.Ok(JsonConvert.SerializeObject(appsResponse), "application/json");
        }
    }
}
