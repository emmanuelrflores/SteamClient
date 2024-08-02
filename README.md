# Steam Client

## Purpose
Application created for a Database class which collects data to populate a neo4j graph database. It collects the following

* Steam Players - Starting with Steam API user, will retrieve friends, and friends of friends (depending on the depth)
* Games - Game metadata and scores
* Publishers
* Developers
* Genres
* Categories

It will also generate the relation of steam players and the games they own. It will also create relationships for games, it's publishers, genres, categories, etc. 

## Pre-req and set up

Get your Steam Api Key here: [Steam Web API Key](https://steamcommunity.com/dev/apikey). Keep it secret, keep it safe.

Get your Steam ID from here: [Steam Account Page](https://store.steampowered.com/account/)

Create a json config file like this: 
```Json
{
  "apiKey": "<your ApkKey",
  "steamId": <your steamId>,
  "depth": 3,
  "playerLimit": 30
}
```
Or edit the [user-config-template.json](/SteamClient/user-config-template.json) and rename it `user-config.json` or something you'll remember. Save the file in the same directory as the `SteamClient.dll`

If you're on a Mac and haven't done so, install .NET framework with one of these options so it can run.

* [Microsoft: Install .NET on MacOS](https://learn.microsoft.com/en-us/dotnet/core/install/macos)
* [Brew](https://formulae.brew.sh/cask/dotnet)

## Build and run

Build as you would any .NET application. 

Run in the terminal:

```terminal
> dotnet SteamClient.dll user-config.json
```
You should see:
```
Config loaded successfully.
Going 3 layers deep into players list. A larger depth will pull a lot of players
Friends will be limited to 30. Reminder, the higher the limit, the more friends collected.
You are limited to 100,000 calls per day, keep that in mind if you need to rerun things
```

It may take some time to run, but monitor the output. You may see some `401 Unauthorized` errors, but you can ignore those. If you consistently see the error then something went wrong and you should stop the run. 

A lot of files will be created in your `{Debug|Release}\bin\Debug\net7.0\`. They are as follows

Relation Lists: 
all the relations between `x` and `y`. This is for creating relations for Neo4j
* all-friend-relation-list.json
* all-game-category-relation-list.json
* all-game-developer-relation-list.json
* all-game-genre-relation-list.json
* all-game-publisher-relation-list.json
* all-owned-games-relation-list.json

Specific item lists
* steam-app-details-list.json
* steam-category-list.json
* steam-developers-list.json
* steam-genre-list.json
* steam-player-list.json
* steam-publisher-list.json

Below is the call for all of the Steam games. 
https://api.steampowered.com/ISteamApps/GetAppList/v2/?

## NOTE
This was created for a class in Spring of 2023. The Steam API is not openly documented by Valve and is subject to change, so this may not work far into the future. 