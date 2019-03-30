using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class ActivityRequest
    {
        [JsonProperty("activity")]
        public Activity Activity { get; set; }
    }
}
