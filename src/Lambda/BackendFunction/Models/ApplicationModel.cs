using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace ApiFunction.Models
{
    public record ApplicationModel
    {
        [DynamoDBHashKey]
        public string AppName { get; set; }

        [DynamoDBProperty]
        public string Description { get; set; }

        [DynamoDBProperty]
        public string MatIcon { get; set; }

        [DynamoDBProperty]
        public string IconBg { get; set; }
        // #ffffff hex key for icon background

        [DynamoDBProperty]
        public string IconColour { get; set; }
        // #ffffff hex key for icon Colour

        [DynamoDBProperty]
        public string LoginUrl { get; set; }

        [DynamoDBProperty]
        public string WhitelistUsers { get; set; }
        // The above property allows you to whitelist display of an application to certain users
        // Should be inserted in dynamo as comma seperate email addresses e.g. user1@email.io,user2@email.io
    }
}
