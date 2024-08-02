using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBOwnedGame
    {
        [JsonProperty("appId")]
        public uint AppId { get; set; }

        [JsonProperty("playtimeForever")]
        public TimeSpan PlaytimeForever { get; set; }

        [JsonProperty("playtimeLastTwoWeeks")]
        public TimeSpan? PlaytimeLastTwoWeeks { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("steamId")]
        public ulong SteamId { get; set; } 
    }
}
