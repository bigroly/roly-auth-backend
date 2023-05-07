using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Models
{
    public record GetAppsResponse
    {
        public List<ApplicationModel> Apps { get; set; }
    }
}
