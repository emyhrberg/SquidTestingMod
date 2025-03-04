﻿// using System;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Threading.Tasks;
// using log4net;
// using log4net.Appender;
// using Microsoft.Xna.Framework.Graphics;
// using ReLogic.Content;
// using SquidTestingMod.Common.Configs;
// using SquidTestingMod.Helpers;
// using Terraria;
// using Terraria.GameContent.UI.Elements;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Terraria.UI;

// namespace SquidTestingMod.UI
// {
//     public class RefreshButton(Asset<Texture2D> _image, string hoverText) : BaseButton(_image, hoverText)
//     {
//         private void ClearLogFile()
//         {
//             // Get all file appenders from log4net's repository
//             var appenders = LogManager.GetRepository().GetAppenders().OfType<FileAppender>();

//             foreach (var appender in appenders)
//             {
//                 // Close the file to release the lock.
//                 var closeFileMethod = typeof(FileAppender).GetMethod("CloseFile", BindingFlags.NonPublic | BindingFlags.Instance);
//                 closeFileMethod?.Invoke(appender, null);

//                 // Overwrite the file with an empty string.
//                 File.WriteAllText(appender.File, string.Empty);

//                 // Reactivate the appender so that logging resumes.
//                 appender.ActivateOptions();
//             }
//         }

//         public override void LeftClick(UIMouseEvent evt)
//         {
//             Config c = ModContent.GetInstance<Config>();

//             // 1) Clear client.log if needed
//             if (c.ClearClientLogOnReload)
//             {
//                 Log.Info("Clearing client logs....");
//                 ClearLogFile();
//             }

//             // 2) Exit world (maybe no longer needed if server is killed but idk)
//             ExitWorld(c);

//             // 3) Navigate to Develop Mods
//             object modSourcesInstance = NavigateToDevelopMods();

//             // 4) Build and reload
//             BuildReload(modSourcesInstance);

//             // 5) Autoload player into world
//         }

//         private void EnterSingleplayerWorld()
//         {
//             Log.Info("[RefreshButton] EnterSingleplayerWorld() called!");

//             // Load worlds and players
//             Main.LoadWorlds();
//             Main.LoadPlayers();

//             // Get first world and player
//             var world = Main.WorldList.FirstOrDefault();
//             var player = Main.PlayerList.FirstOrDefault();

//             // Select world and player
//             Main.ActiveWorldFileData = world;
//             Main.SelectPlayer(player);
//             Log.Info($"Selected world: {world.Name}, Selected player: {player.Name}");

//             // Enter world with the selected world and player
//             WorldGen.playWorld();
//         }

//         private static void ExitWorld(Config c)
//         {
//             if (c.SaveWorldOnReload)
//             {
//                 Log.Warn("Saving and quitting...");
//                 WorldGen.SaveAndQuit(() =>
//                 {
//                     Log.Info("World saved and quit successfully.");
//                 });
//             }
//             else
//             {
//                 Log.Warn("Just quitting...");
//                 WorldGen.JustQuit();
//             }
//         }

//         private static void BuildReload(object modSourcesInstance)
//         {
//             if (modSourcesInstance == null)
//             {
//                 Log.Warn("modSourcesInstance is null.");
//                 return;
//             }

//             var itemsField = modSourcesInstance.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
//             if (itemsField == null)
//             {
//                 Log.Warn("_items field not found.");
//                 return;
//             }

//             var items = (System.Collections.IEnumerable)itemsField.GetValue(modSourcesInstance);
//             if (items == null)
//             {
//                 Log.Warn("_items is null.");
//                 return;
//             }

//             object modSourceItem = null;
//             string modNameFound = "";

//             foreach (var item in items)
//             {
//                 if (item.GetType().Name == "UIModSourceItem")
//                 {
//                     // Extract and log the mod name
//                     var modNameField = item.GetType().GetField("_modName", BindingFlags.NonPublic | BindingFlags.Instance);
//                     if (modNameField != null)
//                     {
//                         var modNameValue = modNameField.GetValue(item);
//                         if (modNameValue is UIText uiText)
//                         {
//                             string modName = uiText.Text;
//                             Log.Info($"Mod Name: {modName}");
//                             Config c = ModContent.GetInstance<Config>();
//                             if (modName == c.ModToReload)
//                             {
//                                 modSourceItem = item;
//                                 modNameFound = modName;
//                                 break;
//                             }
//                         }
//                         else
//                         {
//                             Log.Warn("Mod name is not a UIText.");
//                         }
//                     }
//                 }
//             }

//             if (modSourceItem == null)
//             {
//                 Log.Warn("UIModSourceItem not found.");
//                 return;
//             }

//             var BuildAndReloadMethod = modSourceItem.GetType().GetMethod("BuildAndReload", BindingFlags.NonPublic | BindingFlags.Instance);
//             if (BuildAndReloadMethod == null)
//             {
//                 Log.Warn("BuildAndReload method not found.");
//                 return;
//             }

//             Log.Info($"Invoking BuildAndReload method with {modNameFound} UIModSourceItem...");
//             BuildAndReloadMethod.Invoke(modSourceItem, [null, null]);
//         }

//         private static object NavigateToDevelopMods()
//         {
//             try
//             {
//                 Log.Info("Attempting to navigate to Develop Mods...");

//                 Assembly tModLoaderAssembly = typeof(Main).Assembly;
//                 Type interfaceType = tModLoaderAssembly.GetType("Terraria.ModLoader.UI.Interface");

//                 FieldInfo modSourcesField = interfaceType.GetField("modSources", BindingFlags.NonPublic | BindingFlags.Static);
//                 object modSourcesInstance = modSourcesField?.GetValue(null);

//                 FieldInfo modSourcesIDField = interfaceType.GetField("modSourcesID", BindingFlags.NonPublic | BindingFlags.Static);
//                 int modSourcesID = (int)(modSourcesIDField?.GetValue(null) ?? -1);
//                 Log.Info("modSourcesID: " + modSourcesID);

//                 Main.menuMode = modSourcesID;

//                 Log.Info($"Successfully navigated to Develop Mods (MenuMode: {modSourcesID}).");

//                 return modSourcesInstance;
//             }
//             catch (Exception ex)
//             {
//                 Log.Error($"Error navigating to Develop Mods: {ex.Message}\n{ex.StackTrace}");
//                 return null;
//             }
//         }
//     }
// }