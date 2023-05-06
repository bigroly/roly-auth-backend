using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Models
{
    public record RegistrationRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
