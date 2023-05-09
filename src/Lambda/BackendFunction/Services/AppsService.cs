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
                        Name = "CabinConnect",
                        Description = "This app allows you to configure a list of rooms or beds and set up a booking portal for people you would like to be able to access it (usually as members).",
                        MatIcon = "single_bed",
                        LoginUrl = "https://cabinconnect.rolyapps.com/login"
                    }
                }
            };

            return _utils.Ok(JsonConvert.SerializeObject(appsResponse), "application/json");
        }
    }
}
