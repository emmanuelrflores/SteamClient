using Newtonsoft.Json;
using Steam.Models.SteamStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamClient.Model
{
    public class GDBAppDetail
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("steamAppId")]
        public uint SteamAppId { get; set; }

        [JsonProperty("dlc")]
        public uint[] Dlc { get; set; }

        [JsonProperty("developers")]
        public string[] Developers { get; set; }

        [JsonProperty("publishers")]
        public string[] Publishers { get; set; }

        [JsonProperty("metacriticScore")]
        public uint MetacriticScore { get; set; }

        [JsonProperty("metacritic", NullValueHandling = NullValueHandling.Ignore)]
        public StoreMetacriticModel MetacriticModel { get; set; }

        [JsonProperty("categories")]
        public StoreCategoryModel[] Categories { get; set; }

        [JsonProperty("genres")]
        public StoreGenreModel[] Genres { get; set; }
    }
}
