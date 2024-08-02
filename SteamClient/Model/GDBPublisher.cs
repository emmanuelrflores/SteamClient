using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBPublisher
    {
        [JsonProperty("publisher")]
        public string Publisher { get; set; }

        [JsonProperty("appId")]
        public uint AppId { get; set; }
    }
}
