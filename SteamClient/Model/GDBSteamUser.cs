using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBSteamUser
    {
        [JsonProperty("accountCreationDate")]
        public DateTime AccountCreateDate { get; set; }
        [JsonProperty("steamId")]
        public ulong SteamId { get; set; }
    }
}
