using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBGameCatRelation
    {
        [JsonProperty("categeoryId")]
        public uint CategoryId { get; set; }

        [JsonProperty("steamAppId")]
        public uint SteamAppId { get; set; }
    }
}
