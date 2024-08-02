
using Newtonsoft.Json;
using SteamClient.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SteamClient
{
    internal class JsonHelper
    {
        private static readonly JsonSerializerSettings _options =
        new() { NullValueHandling = NullValueHandling.Ignore };
        public static T? ReadJsonToObject<T>(string filePath)
        {
            if(!Path.Exists(filePath))
            {
                return default(T);
            }
            using StreamReader stream = new(filePath);
            string json = stream.ReadToEnd();
            if (json == null)
            {
                throw new FileLoadException("file had no contents");
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void WriteJson(object item, string filename)
        {
            if (item == null) { throw new ArgumentNullException(nameof(item)); }
            string jsonString = JsonConvert.SerializeObject(item, Formatting.Indented, _options);
            using StreamWriter outputFile = new(filename + (filename.Contains(".json") ? "" : ".json"));
            outputFile.WriteLine(jsonString);
        }

        public static void WriteUserToFile(GDBSteamUser user)
        {
            string userListPath = "UserList.json";

            if (!File.Exists(userListPath))
            {
                string jsonString = JsonConvert.SerializeObject(user, Formatting.Indented, _options);
                using StreamWriter outputFile = new(userListPath);
                outputFile.WriteLine(jsonString);
            }
            else
            {
                using StreamWriter outputFile = File.AppendText(userListPath);
                string jsonString = JsonConvert.SerializeObject(user, Formatting.Indented, _options);
                outputFile.WriteLine(jsonString);
            }
        }

    }
}
