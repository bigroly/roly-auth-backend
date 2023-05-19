using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Models
{
    public record LoginWithTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
