using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Models
{
    public record LoginResponse
    {
        public string AccessToken { get; set; }
        public long Expiry { get ; set; }
        public string RefreshToken { get; set; }
    }
}
