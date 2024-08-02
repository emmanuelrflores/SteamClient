
using SteamClient.Model;

namespace SteamClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // uncomment if you'd rather run from VS Code or Studio
            //args = new[] { "../../../user-config.json"};
            if(args.Length == 0)
            {
                throw new ArgumentException("Need at least 1 argument and it should be the config file");
            }
            string arg1 = args[0];

            if(arg1 == null || !arg1.Contains("user-config.json")) {
                throw new ArgumentException("arg 1 should be user config named user-config.json");
            }

            UserConfig? userConfig = 
                JsonHelper.ReadJsonToObject<UserConfig>(arg1) ?? 
                throw new Exception("Unable to load config file; no data was present");

            Console.WriteLine("Config loaded successfully.");
            Console.WriteLine("Going " + userConfig.Depth + " layers deep " +
                "into players list. A larger depth will pull a lot of players");
            Console.WriteLine("Friends will be limited to " + userConfig.PlayerLimit +
                ". Reminder, the higher the limit, the more friends collected.");
            Console.WriteLine("You are limited to 100,000 calls per day, keep " +
                "that in mind if you need to rerun things");
            Console.WriteLine("*****");
            Console.WriteLine("Note: a user-list.json file is created as a backup " +
                "to store player info in case the app crashes while collecting " +
                "data. If the application runs all the way through, you can " +
                "delete that file.");
            SteamAPI steamApi = new(userConfig);
            if (userConfig.LoadPlayers)
            {
                steamApi.GenerateSteamDataFromUser(userConfig.SteamId);
            }
            
            if (userConfig.LoadGameInfo)
            {
                Console.WriteLine("Processing players game list to get game info");
                steamApi.BuildGameInfo();
                Console.WriteLine("Game info has been built");
            }
        }
    }
}