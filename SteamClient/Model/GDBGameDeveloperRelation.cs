using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBGameDeveloperRelation
    {
        [JsonProperty("steamAppId")]
        public uint SteamAppId { get; set; }

        [JsonProperty("developer")]
        public string Developer { get; set; }
    }
}
