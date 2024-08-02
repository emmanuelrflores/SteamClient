using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class UserConfig
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("steamId")]
        public ulong SteamId { get; set; }

        [JsonProperty("depth")]
        public int Depth { get; set; }

        [JsonProperty("playerLimit")]
        public int PlayerLimit { get; set; }

        [JsonProperty("loadPlayers")]
        public bool LoadPlayers { get; set; }

        [JsonProperty("loadGameInfo")]
        public bool LoadGameInfo { get; set; }

    }
}
