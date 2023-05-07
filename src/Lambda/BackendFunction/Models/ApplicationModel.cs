using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFunction.Models
{
    public record ApplicationModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LoginUrl { get; set; }
    }
}
