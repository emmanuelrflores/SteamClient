using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBGenre
    {
        [JsonProperty("id")]
        public uint Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
