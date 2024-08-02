using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Steam.Models.DOTA2;
using Steam.Models.SteamCommunity;
using Steam.Models.SteamStore;
using SteamClient.Model;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Models;
using SteamWebAPI2.Utilities;

namespace SteamClient
{
    public class SteamAPI
    {
        private SteamWebInterfaceFactory webInterfaceFactory;
        private readonly UserConfig userConfig;
        private static HttpClient client;
        private List<GDBSteamUser> allUserList;
        private List<GDBFriendModel> allFriendsList;
        private List<GDBOwnedGame> allOwnedGames;
        private List<GDBGameCatRelation> allGDBGameCatRelations;
        private List<GDBGameGenreRelation> allGDBGameGenreRelations;
        private List<GDBGameDeveloperRelation> allGDBGameDeveloperRelations;
        private List<GDBGamePublisherRelation> allGDBGamePublisherRelations;
        /// <summary>
        /// The depth of to go down a players friend list. Defaults to 3
        /// </summary>
        private int Depth { get; set; }
        /// <summary>
        /// How many friends to process from a friends list. Defaults to 30
        /// </summary>
        private int FriendListLimit { get; set; }

        private static SteamUser SteamUserInterface { get; set; }

        private static PlayerService PlayerServiceInterface { get; set; }

        private static SteamStore SteamStoreInterface { get; set; }

        public SteamAPI(UserConfig userConfig)
        {
            this.userConfig = userConfig;
            webInterfaceFactory = new SteamWebInterfaceFactory(userConfig.ApiKey);
            client = new HttpClient();
            allUserList = new List<GDBSteamUser>();
            allFriendsList = new List<GDBFriendModel>();
            allOwnedGames = new List<GDBOwnedGame>();
            allGDBGameCatRelations = new List<GDBGameCatRelation>();
            allGDBGameGenreRelations = new();
            allGDBGameDeveloperRelations = new();
            allGDBGamePublisherRelations = new();
            Depth = userConfig.Depth;
            FriendListLimit = userConfig.PlayerLimit;
            // this will map to the ISteamUser endpoint
            // note that you have full control over HttpClient lifecycle here
            SteamUserInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>(client);
            PlayerServiceInterface = webInterfaceFactory.CreateSteamWebInterface<PlayerService>(client);
            SteamStoreInterface = webInterfaceFactory.CreateSteamStoreInterface(client);
        }

        public void GenerateSteamDataFromUser(ulong steamId)
        {
            try
            {
                Console.WriteLine("Starting to generate users.");
                BuildSteamUserList(steamId);
                Console.WriteLine("Finished going through players. Collected "
                    + allUserList.Count + " players");
                Console.WriteLine("Saving player list to steam-player-list.json. " +
                    "You can delete user-list.json.");
                JsonHelper.WriteJson(allUserList, "steam-player-list");

                Console.WriteLine("Saving all friends relation to all-friends-relation-list.json");
                JsonHelper.WriteJson(allFriendsList, "all-friend-relation-list");

                Console.WriteLine("Saving all friends relation to all-owned-games-relation-list.json");
                JsonHelper.WriteJson(allOwnedGames, "all-owned-games-relation-list");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown when trying to build user " +
                    "list.");
                Console.WriteLine(ex);
                throw;
            }
        }

        private void BuildSteamUserList(ulong steamUserId)
        {
            DoListBuild(steamUserId, 0, Depth);
        }

        private void DoListBuild(ulong steamUserId, int currDepth, int depthLimit)
        {
            if (currDepth >= depthLimit)
            {
                return;
            }

            client ??= new HttpClient();
            // this will map to ISteamUser/GetPlayerSummaries method in the Steam Web API
            // see PlayerSummaryResultContainer.cs for response documentation
            ISteamWebResponse<PlayerSummaryModel> playerSummaryResponse;

            try
            {
                playerSummaryResponse = SteamUserInterface.GetPlayerSummaryAsync(steamUserId).Result;
                if (playerSummaryResponse == null)
                {
                    Console.WriteLine("playerSummaryResponse returned null. No steam Id info");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown when trying to get player summary for " + steamUserId);
                Console.WriteLine(ex);
                return;
            }

            var playerSummaryData = playerSummaryResponse.Data;

            GDBSteamUser gdbSteamuser = new()
            {
                SteamId = playerSummaryData.SteamId,
                AccountCreateDate = playerSummaryData.AccountCreatedDate,
            };
            // writing to file as a back up in case the run fails
            JsonHelper.WriteUserToFile(gdbSteamuser);

            // if we have't seen the user, then return. We've already 
            // done a search on the friends
            if (allUserList.Contains(gdbSteamuser))
            {
                return;
            }

            // if it's private then just return
            if (playerSummaryData.ProfileVisibility != ProfileVisibility.Public)
            {
                Console.WriteLine("Profle set to '" + playerSummaryData.ProfileVisibility.ToString() + "'");
                Console.WriteLine("Skipping steam player.");
                return;
            }
            allUserList.Add(gdbSteamuser);

            BuildGameListForUser(steamUserId);
            // this will map to ISteamUser/GetFriendsListAsync method in the Steam Web API
            // see FriendListResultContainer.cs for response documentation
            IReadOnlyCollection<FriendModel> friendsList;
            try
            {
                var friendsListResponse = SteamUserInterface.GetFriendsListAsync(steamUserId).Result;
                friendsList = friendsListResponse.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown on '" + steamUserId + "'");
                Console.WriteLine(playerSummaryData);
                Console.WriteLine(ex);
                return;
            }
            if (friendsList.Count == 0)
            {
                Console.WriteLine("Player had no friends. A little sad, but understandable. Moving on.");
                return;
            }

            // Only make friends list if we're not at the last level depthLimit - 1
            if (currDepth < depthLimit)
            {
                List<GDBFriendModel> newFriendList = friendsList.Select(friend =>
                new GDBFriendModel
                {
                    FriendSteamId = friend.SteamId,
                    PlayerSteamId = steamUserId,
                    Relationship = friend.Relationship,
                    FriendSince = friend.FriendSince
                }).ToList();
                allFriendsList.AddRange(newFriendList);

                int friendCount = newFriendList.Count > FriendListLimit ? FriendListLimit : newFriendList.Count;
                int count = 0;
                foreach (GDBFriendModel friend in newFriendList)
                {
                    if (count > friendCount)
                    {
                        return;
                    }

                    if (allUserList.Any(player => player.SteamId == friend.FriendSteamId))
                    {
                        continue;
                    }
                    // now go into the friend and get that person's friends
                    // make sure to increment the currDepth so we don't go on forever
                    // and buildout the entire player base. 
                    DoListBuild(friend.FriendSteamId, currDepth + 1, depthLimit);
                    count++;
                }
            }
        }

        private void BuildGameListForUser(ulong steamId)
        {
            client ??= new HttpClient();

            var ownedGames = PlayerServiceInterface.GetOwnedGamesAsync(steamId).Result;
            var gameList = ownedGames.Data;

            if (ownedGames == null || ownedGames.Data == null)
            {
                Console.WriteLine("Player oddly didn't have a games list");
                return;
            }

            var gdbGameList = gameList.OwnedGames.Select(game =>
            {
                return new GDBOwnedGame()
                {
                    SteamId = steamId,
                    AppId = game.AppId,
                    PlaytimeForever = game.PlaytimeForever,
                    PlaytimeLastTwoWeeks = game.PlaytimeLastTwoWeeks,
                    Name = game.Name
                };
            }).ToList();
            
            if (gdbGameList.Count > 0)
            {
                allOwnedGames.AddRange(gdbGameList);
            }
        }

        public void BuildGameInfo()
        {
            client ??= new HttpClient();

            if (allUserList == null || allUserList.Count == 0)
            {
                var list = JsonHelper.ReadJsonToObject<List<GDBSteamUser>>("steam-player-list.json") ??
                    throw new Exception("User list did not exist, nor did the json file. Can't load games");

                allUserList = list;
            }


            // store game we've seen
            List<GDBGenre>? genreList = new();
            List<GDBCategory> categoryList = new();
            List<GDBPublisher> publishersList = new();
            List<string> developerList = new();

            List<GDBAppDetail>? appDetailList = JsonHelper.ReadJsonToObject<List<GDBAppDetail>>("steam-app-details-list.json") ?? new();
            // if loaded, then keep track of the count
            // if it hasn't changed, don't rewrite the file
            int appDetailListCount = appDetailList.Count;
            List<uint> failedAppIds = new();
            // set up some call limits so we don't get rate limited and/or blocked
            int callLimit = 15;
            int callCount = 0;
            bool makeApiCalls = true;

            List<GDBOwnedGame>? allOwnedGames = JsonHelper.ReadJsonToObject<List<GDBOwnedGame>>("all-owned-games-relation-list.json");

            if(allOwnedGames == null || allOwnedGames.Count == 0)
            {
                Console.WriteLine("Unable to load all owned games. Does it exist? " +
                    "May need to run the user data collection");
                return;
            }
            allOwnedGames = allOwnedGames.GroupBy(x => x.AppId).Select(x => x.First()).ToList();


            foreach (var game in allOwnedGames)
            {
                GDBAppDetail? appDetail;
                if (appDetailList.Exists(x => x.SteamAppId == game.AppId))
                {
                    appDetail = appDetailList.Find(app => app.SteamAppId == game.AppId);

                    if (appDetail == null)
                    {
                        Console.WriteLine("For some reason, the app existed, but returned null");
                        continue;
                    }
                    if (appDetail.MetacriticModel != null)
                    {
                        appDetail.MetacriticScore = appDetail.MetacriticModel.Score;
                    }
                }
                else
                {
                    if (failedAppIds.Contains(game.AppId))
                    {
                        Console.WriteLine("AppId " + game.AppId + " failed previously, skipping call");
                        continue;
                    }
                    StoreAppDetailsDataModel? storeAppDetailsDataModel = null;

                    try
                    {
                        if (!makeApiCalls)
                        {
                            Console.WriteLine("Skipping call to get game data. We've been rate limited");
                            Console.WriteLine("Make sure to wait 1 hour before trying again.");
                            continue;
                        }
                        if (callCount++ == callLimit)
                        {
                            Console.WriteLine("hit call limit. Waiting 25 seconds");
                            Thread.Sleep(25000);
                            callCount = 0;
                            Console.WriteLine("Continuing with calls...hopefully we don't get rate limited");
                        }
                        Console.WriteLine("Making api call for: " + game.AppId);
                        storeAppDetailsDataModel = SteamStoreInterface.GetStoreAppDetailsAsync(game.AppId, cc: "US", language: "en").Result;
                    }
                    catch (Exception ex)
                    {
                        failedAppIds.Add(game.AppId);
                        Console.WriteLine("Unable to get app detail");
                        Console.WriteLine("AppID: " + game.AppId);

                        if (ex.Message.Contains("429"))
                        {
                            Console.WriteLine("Hit API endpoint rate limit. No longer making calls");
                            DateTime dateTime = DateTime.Now;
                            Console.WriteLine("Wait one hour from " + dateTime.ToLocalTime().ToShortTimeString());
                            makeApiCalls = false;
                        }
                        if (!failedAppIds.Contains(game.AppId))
                        {
                            failedAppIds.Add(game.AppId);
                        }
                        continue;
                    }

                    if (storeAppDetailsDataModel == null)
                    {
                        if (!failedAppIds.Contains(game.AppId))
                        {
                            failedAppIds.Add(game.AppId);
                        }
                        Console.WriteLine("Unable to get game details");
                        continue;
                    }

                    // We don't want all the details, just save this stuff
                    uint score = 0;
                    if (storeAppDetailsDataModel.Metacritic != null)
                    {
                        score = storeAppDetailsDataModel.Metacritic.Score;
                    }
                    appDetail = new()
                    {
                        Type = storeAppDetailsDataModel.Type,
                        Name = storeAppDetailsDataModel.Name,
                        SteamAppId = storeAppDetailsDataModel.SteamAppId,
                        Dlc = storeAppDetailsDataModel.Dlc,
                        MetacriticScore = score,
                        Developers = storeAppDetailsDataModel.Developers,
                        Publishers = storeAppDetailsDataModel.Publishers,
                        Categories = storeAppDetailsDataModel.Categories,
                        Genres = storeAppDetailsDataModel.Genres,
                    };

                    appDetailList.Add(appDetail);
                }

                ExtractGameRelations(game, appDetail);

                foreach (var category in appDetail.Categories)
                {
                    if (categoryList.Any(x => x.Id == category.Id))
                    {
                        continue;
                    }

                    categoryList.Add(new GDBCategory()
                    {
                        Id = category.Id,
                        Description = category.Description,
                    });
                }

                foreach (var genre in appDetail.Genres)
                {
                    if (genreList.Any(x => x.Id == genre.Id))
                    {
                        continue;
                    }

                    genreList.Add(new GDBGenre()
                    {
                        Id = genre.Id,
                        Description = genre.Description,
                    });
                }

                foreach (var publisher in appDetail.Publishers)
                {
                    if (publishersList.Any(x => x.Publisher == publisher))
                    {
                        continue;
                    }

                    publishersList.Add(new GDBPublisher()
                    {
                        Publisher = publisher
                    });
                }

                foreach (var developer in appDetail.Developers)
                {
                    if (developerList.Any(x => x == developer))
                    {
                        continue;
                    }

                    developerList.Add(developer);
                }

            }

            if (appDetailList.Count > 0)
            {
                JsonHelper.WriteJson(appDetailList, "steam-app-details-list");
            }

            if (genreList.Count > 0)
            {
                JsonHelper.WriteJson(genreList, "steam-genre-list");
            }

            if (categoryList.Count > 0)
            {
                JsonHelper.WriteJson(categoryList, "steam-category-list");
            }

            if (publishersList.Count > 0)
            {
                JsonHelper.WriteJson(publishersList, "steam-publishers-list");
            }

            if (developerList.Count > 0)
            {
                JsonHelper.WriteJson(developerList, "steam-developers-list");
            }

            if (allGDBGameCatRelations.Count > 0)
            {
                Console.WriteLine("Saving all friends relation to all-game-cateogry-relation-list.json");
                JsonHelper.WriteJson(allGDBGameCatRelations, "all-game-category-relation-list");
            }

            if (allGDBGameGenreRelations.Count > 0)
            {
                Console.WriteLine("Saving all friends relation to all-game-genre-relation-list.json");
                JsonHelper.WriteJson(allGDBGameGenreRelations, "all-game-genre-relation-list");
            }

            if (allGDBGameDeveloperRelations.Count > 0)
            {
                Console.WriteLine("Saving all friends relation to all-game-developer-relation-list.json");
                JsonHelper.WriteJson(allGDBGameDeveloperRelations, "all-game-developer-relation-list");
            }

            if (allGDBGamePublisherRelations.Count > 0)
            {
                Console.WriteLine("Saving all friends relation to all-game-publisher-relation-list.json");
                JsonHelper.WriteJson(allGDBGamePublisherRelations, "all-game-publsher-relation-list");
            }
        }

        private void ExtractGameRelations(GDBOwnedGame game, GDBAppDetail appDetail)
        {
            string[] developers = appDetail.Developers;
            string[] publishers = appDetail.Publishers;
            StoreCategoryModel[] categroies = appDetail.Categories;
            StoreGenreModel[] genres = appDetail.Genres;

            var gdbCatRelation = categroies.Select(cat => new GDBGameCatRelation
            {
                CategoryId = cat.Id,
                SteamAppId = game.AppId
            }).ToList();

            foreach (GDBGameCatRelation catRel in gdbCatRelation)
            {
                if (allGDBGameCatRelations.Contains(catRel))
                {
                    continue;
                }
                allGDBGameCatRelations.Add(catRel);
            }

            var genreRelation = genres.Select(genre => new GDBGameGenreRelation
            {
                GenreId = genre.Id,
                SteamAppId = game.AppId
            }).ToList();

            foreach (GDBGameGenreRelation genRel in genreRelation)
            {
                if (allGDBGameGenreRelations.Contains(genRel))
                {
                    continue;
                }
                allGDBGameGenreRelations.Add(genRel);
            }            

            var devRelation = developers.Select(dev => new GDBGameDeveloperRelation
            {
                Developer = dev,
                SteamAppId = game.AppId
            }).ToList();
            foreach (GDBGameDeveloperRelation devRel in devRelation)
            {
                if (allGDBGameDeveloperRelations.Contains(devRel))
                {
                    continue;
                }
                allGDBGameDeveloperRelations.Add(devRel);
            }

            var pubRelation = developers.Select(pub => new GDBGamePublisherRelation
            {
                Publisher = pub,
                SteamAppId = game.AppId
            }).ToList();
            foreach (GDBGamePublisherRelation pubRel in pubRelation)
            {
                if (allGDBGamePublisherRelations.Contains(pubRel))
                {
                    continue;
                }
                allGDBGamePublisherRelations.Add(pubRel);
            }
        }
    }
}

