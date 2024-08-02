using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBGamePublisherRelation
    {
        [JsonProperty("steamAppId")]
        public uint SteamAppId { get; set; }

        [JsonProperty("publisher")]
        public string Publisher { get; set; }
    }
}
