using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    internal class GDBFriendModel
    {
        [JsonProperty("friendSteamId")]
        public ulong FriendSteamId { get; set; }

        [JsonProperty("playerSteamId")]
        public ulong PlayerSteamId { get; set; }

        [JsonProperty("relationship")]
        public string Relationship { get; set; }

        [JsonProperty("friendSince")]
        public DateTime FriendSince { get; set; }
    }
}
