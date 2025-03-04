// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Net.NetworkInformation;
// using System.Reflection;
// using System.Security.Cryptography.Pkcs;
// using SquidTestingMod.Common.Configs;
// using SquidTestingMod.Helpers;
// using Terraria;
// using Terraria.GameContent.UI.States;
// using Terraria.ID;
// using Terraria.IO;
// using Terraria.ModLoader;

// namespace SquidTestingMod.Common.Systems
// {
//     /// <summary>
//     /// This system will do the following when mod has been loaded (when you click Build & Reload in tModLoader):
//     /// 1. restart the server with the same world, port, password and other settings.
//     /// 2. connect to the server as a client once the server is ready.
//     /// </summary>
//     [Autoload(Side = ModSide.Client)]

//     public class AutoloadMultiplayerSystem : ModSystem
//     {

//         // Variables
//         private int serverProcessID;

//         public override void OnModLoad()
//         {
//             // Get the OnSuccessfulLoad field using reflection
//             FieldInfo onSuccessfulLoadField = typeof(ModLoader).GetField("OnSuccessfulLoad", BindingFlags.NonPublic | BindingFlags.Static);

//             if (onSuccessfulLoadField != null)
//             {
//                 Action onSuccessfulLoad = (Action)onSuccessfulLoadField.GetValue(null);
//                 Config c = ModContent.GetInstance<Config>();
//                 if (c.AutoloadWorld == "Multiplayer")
//                 {
//                     onSuccessfulLoad += StartServerAndEnterMultiplayerWorld;
//                 }
//                 // Set the modified delegate back to the field
//                 onSuccessfulLoadField.SetValue(null, onSuccessfulLoad);
//             }
//             else
//             {
//                 Mod.Logger.Warn("Failed to access OnSuccessfulLoad field.");
//             }
//         }

//         public override void Unload() // reset some hooks
//         {
//             typeof(ModLoader).GetField("OnSuccessfulLoad", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, null);
//         }

//         private void StartServer()
//         {
//             try
//             {
//                 // get filenames
//                 Config c = ModContent.GetInstance<Config>();
//                 // string worldName = c.Reload.WorldToLoad;
//                 string worldName = "MyWorld";

//                 Main.LoadWorlds();
//                 var choosenWorlds = Main.WorldList.ToList().Where(w => w.Name == worldName).ToList();
//                 string fileNameWorld = choosenWorlds[0].Path;

//                 string fileNameStartProcess = @"C:\Program Files (x86)\Steam\steamapps\common\tModLoader\start-tModLoaderServer.bat";

//                 // create a process
//                 ProcessStartInfo process = new(fileNameStartProcess)
//                 {
//                     UseShellExecute = true,
//                     Arguments = $"-nosteam -world {fileNameWorld}"
//                 };

//                 // start the process
//                 Process serverProcess = Process.Start(process);
//                 serverProcessID = serverProcess.Id;
//                 Log.Info("Server process started with ID: " + serverProcessID + " and name: " + serverProcess.ProcessName);
//             }
//             catch (Exception e)
//             {
//                 // log it
//                 Mod.Logger.Error("Failed to start server!!! C:/Program Files (x86)/Steam/steamapps/common/tModLoader/start-tModLoaderServer.bat" + e.Message);
//                 return;
//             }
//         }

//         private void StartServerAndEnterMultiplayerWorld()
//         {
//             Mod.Logger.Info("AutoloadMultiplayerSystem:StartServerAndEnterMultiplayerWorld called!");

//             Config c = ModContent.GetInstance<Config>();
//             StartServer();

//             Mod.Logger.Info("EnterMultiplayerWorld() called!");
//             Main.LoadWorlds();
//             Main.LoadPlayers();

//             // Players 
//             var journeyPlayers = Main.PlayerList.ToList().Where(IsJourneyPlayer).ToList();
//             var favoritePlayersNonJourney = Main.PlayerList.Where(p => p.IsFavorite).Except(journeyPlayers).ToList();
//             PlayerFileData firstFavorite = favoritePlayersNonJourney[0];

//             // get world with name "MyWorld" since that is the name of the server specified in serverconfig.txt
//             // string WORLD_NAME_IN_CONFIG = c.Reload.WorldToLoad;
//             string WORLD_NAME_IN_CONFIG = "MyWorld";

//             WorldFileData w = Main.WorldList.FirstOrDefault(world => world.Name == WORLD_NAME_IN_CONFIG);
//             if (w == null || string.IsNullOrEmpty(w.Path))
//             {
//                 Log.Error("Could not find world named " + WORLD_NAME_IN_CONFIG);
//                 return;
//             }

//             Main.SelectPlayer(firstFavorite);
//             Main.ActiveWorldFileData = w;

//             // Play the selected world in multiplayer mode
//             // Connect to server IP
//             Ping pingSender = new();
//             PingOptions options = new();
//             options.DontFragment = true; // prevent packet from splitting into smaller packets
//             string data = "a"; // dummy data to send because the Send method requires it
//             byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data); // convert string to byte array
//             int timeout = 2000; // 120 ms timeout before the ping request is considered failed

//             // Ping the server IP using the server's IP address
//             PingReply reply = null;
//             try
//             {
//                 Netplay.ServerIP = new System.Net.IPAddress([127, 0, 0, 1]); // localhost
//                 reply = pingSender.Send(Netplay.ServerIP, timeout, buffer, options);
//             }
//             catch (PingException ex)
//             {
//                 Mod.Logger.Error($"Ping failed to destination server: {ex}");
//                 return;
//             }

//             if (reply.Status == IPStatus.Success)
//             {
//                 Mod.Logger.Info($"Ping successful to destination server: {reply.Address}");

//                 // set the IP AND PORT (the two necessary fields) for the server
//                 Netplay.ServerIP = new System.Net.IPAddress([127, 0, 0, 1]); // localhost
//                 Netplay.ListenPort = 7777; // default port

//                 Netplay.StartTcpClient(); // start the TCP client which is later used to connect to the server
//             }
//             else
//             {
//                 Mod.Logger.Error($"Ping failed to destination server, possibly timed out: {reply.Status}");
//             }
//         }

//         private bool IsJourneyPlayer(PlayerFileData player) => player.Player.difficulty == GameModeID.Creative;
//     }
// }