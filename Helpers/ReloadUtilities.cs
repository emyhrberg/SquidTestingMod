using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using ModHelper.Common.Configs;
using ModHelper.PacketHandlers;
using ModHelper.UI;
using ModHelper.UI.Buttons;
using ModHelper.UI.Elements;
using MonoMod.RuntimeDetour;
using ReLogic.OS;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.Social;
namespace ModHelper.Helpers
{
    public static class ReloadUtilities
    {
        // Global list of all the mods that should be built and reloaded when a reload is executed
        public static List<string> ModsToReload = [];

        private static int clientsCountInServer;

        private const string pipeNameBeforeRebuild = "ModHelperReloadPipeBeforeRebuild";

        private const string pipeNameAfterRebuild = "ModHelperReloadPipeAfterRebuild";

        /// <summary>
        /// Main function to build and reload all the mods in the ModsToReload list for Singleplayer.
        /// </summary>
        public static async Task SinglePlayerReload()
        {
            // Log.Info("mods to reload 2: " + string.Join(", ", ReloadHelper.ModsToReload));

            // Check if list is empty
            if (!CheckIfModsToReloadIsEmpty())
            {
                Main.NewText("No mods selected, please select mods to reload");
                Log.Warn("No mods selected, please select mods to reload");
                return;
            }

            // Another null check for modstoreload, check if modsources contains any mod
            if (!CheckThatModsExists())
            {
                Main.NewText("No mods were existing with that/those names. Please check the mod names and try again.");
                Log.Warn("No mods were found to reload.");
                return;
            }

            // Prepare client. Use singleplayer by default for now.
            PrepareClient(clientMode: ClientMode.SinglePlayer);

            // Exit the game.
            await ExitWorld();
            // Mod was found, we can Reload
            BuildAndReloadMods();
        }

        /// <summary>
        /// Main function to build and reload all the mods in the ModsToReload list for Multiplayer.<br/>
        /// It sends a packet to the server to inform about reloading mod in multiplayer.
        /// </summary>
        /// <returns></returns>
        public static async Task MultiPlayerMainReload()
        {
            // Log.Info("mods to reload 2: " + string.Join(", ", ReloadHelper.ModsToReload));

            // Check if list is empty
            if (!CheckIfModsToReloadIsEmpty())
            {
                Main.NewText("No mods selected, please select mods to reload");
                Log.Warn("No mods selected, please select mods to reload");
                return;
            }

            // Another null check for modstoreload, check if modsources contains any mod
            if (!CheckThatModsExists())
            {
                Main.NewText("No mods were existing with that/those names. Please check the mod names and try again.");
                Log.Warn("No mods were found to reload.");
                return;
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                PrepareClient(clientMode: ClientMode.MPMajor);

                // Exit the game.
                await ExitWorld();

                CreateServerAfterUnloadHook(ClientDataJsonHelper.WorldID);

                // Mod was found, we can Reload
                BuildAndReloadMods();
            }
            else
            {
                clientsCountInServer = Main.player.Where((p) => p.active).Count() - 1;
                // Prepare client. Use singleplayer by default for now.
                ModNetHandler.RefreshServer.SendReloadMP(255, -1, Conf.C.SaveWorldBeforeReloading);
            }
        }

        /// <summary>
        /// Function to build and reload all the mods in the ModsToReload list for MajorMP client.<br/>
        /// It waits for all clients to finish unloading and then rebuilds the mods.<br/>
        /// After rebuilding, it allows all clients to reload the mods.
        /// </summary>
        /// <returns></returns>
        public static async Task MultiPlayerMajorReload()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Prepare client data
                PrepareClient(clientMode: ClientMode.MPMajor);

                // Exit the game.
                await ExitWorld();

                // Kill the server process
                Main.tServer?.WaitForExit();

                // Create a list of pipes for clients
                List<NamedPipeServerStream> clients = new List<NamedPipeServerStream>();
                List<Task<string?>> clientResponses = new List<Task<string?>>();

                Log.Info($"Waiting for {clientsCountInServer} clients...");

                // Wait for conecting all clients
                for (int i = 0; i < clientsCountInServer; i++)
                {
                    // Creating pipe for each client
                    var pipeServer = new NamedPipeServerStream(pipeNameBeforeRebuild, PipeDirection.InOut, clientsCountInServer);
                    GC.SuppressFinalize(pipeServer);

                    // Wait for connection
                    await pipeServer.WaitForConnectionAsync();
                    clients.Add(pipeServer);
                    Log.Info($"Client {i + 1} connected!");
                }

                // Dispose of all clients
                foreach (var client in clients)
                {
                    client.Close();
                    client.Dispose();
                }

                BuildAndReloadMods(() =>
                {
                    Log.Info("Mod Builded");

                    // After building - reload all other clients
                    List<NamedPipeServerStream> clientsAfterRebuild = new List<NamedPipeServerStream>();
                    Log.Info($"Waiting for {clientsCountInServer} clients...");

                    // Wait for conecting all clients
                    for (int i = 0; i < clientsCountInServer; i++)
                    {
                        // Creating pipe for each client
                        var pipeServer = new NamedPipeServerStream(ReloadUtilities.pipeNameAfterRebuild, PipeDirection.InOut, clientsCountInServer);
                        GC.SuppressFinalize(pipeServer);

                        // Wait for connection
                        pipeServer.WaitForConnection();
                        clientsAfterRebuild.Add(pipeServer);
                        Log.Info($"Client {i + 1} connected after building Yay!");
                    }

                    foreach (var client in clientsAfterRebuild)
                    {
                        client.Close();
                        client.Dispose();
                    }

                    // Creating hook for Unload 
                    CreateServerAfterUnloadHook(ClientDataJsonHelper.WorldID);
                });
            }
        }

        /// <summary>
        /// Function to build and reload all the mods in the ModsToReload list for MinorMP client.<br/>
        /// It starts to reload the mods and then waits for the MajorMP client to finish unloading and then rebuilds the mods.<br/>
        /// </summary>
        /// <returns></returns>
        public static async Task MultiPlayerMinorReload()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Prepare client data
                PrepareClient(ClientMode.MPMinor);

                // Exit the game.
                await ExitWorld();

                // Kill the server process (just in case)
                Main.tServer?.WaitForExit();

                // Creating a hook for Unload to wait for the major client to rebuild mod(s)
                Hook hookForUnload = null;
                hookForUnload = new Hook(typeof(ModLoader).GetMethod("Unload", BindingFlags.NonPublic | BindingFlags.Static), (Func<bool> orig) =>
                {
                    var logger = LogManager.GetLogger("MODHELPER_UNLOAD");

                    bool o = orig();

                    object loadMods = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetField("loadMods", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

                    //UILoadMods loadMods = Terraria.ModLoader.UI.Interface.loadMods;

                    typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.UI.UILoadMods").GetMethod("SetProgressText", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(loadMods, ["Waiting for main client", "Waiting for main client"]);


                    using (var pipeClient = new NamedPipeClientStream(".", pipeNameBeforeRebuild, PipeDirection.InOut))
                    {
                        logger.Info($"Waiting for main client");
                        pipeClient.Connect();
                    }

                    logger.Info("Wait to continue loading");

                    using var pipeClientafterRebuild = new NamedPipeClientStream(".", ReloadUtilities.pipeNameAfterRebuild, PipeDirection.InOut);
                    pipeClientafterRebuild.Connect();

                    logger.Info("Clearing modsDirCache");

                    var cache = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.Core.ModOrganizer").GetField("modsDirCache", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

                    if (cache is IDictionary dictionary)
                    {
                        dictionary.Clear(); // Clears the dictionary without needing LocalMod type
                        Console.WriteLine("Cache cleared successfully.");
                    }

                    logger.Info("Loading mods");

                    hookForUnload?.Dispose();

                    return o;
                });

                //stops GC from deleting it
                GC.SuppressFinalize(hookForUnload);

                ReloadMods();
            }
        }

        /// <summary>
        /// Sets up a hook for creating a server after unloading the mods.
        /// </summary>
        /// <param name="worldIdToLoad">ID of the world to load.</param>
        private static void CreateServerAfterUnloadHook(int worldIdToLoad)
        {
            Hook hookForUnload = null;

            hookForUnload = new Hook(typeof(ModLoader).GetMethod("Unload", BindingFlags.NonPublic | BindingFlags.Static), (Func<bool> orig) =>
            {
                var logger = LogManager.GetLogger("MODHELPER_UNLOAD");

                bool o = orig();

                try
                {
                    Main.LoadWorlds();

                    if (Main.WorldList.Count == 0)
                        throw new Exception("No worlds found.");

                    // Getting Player and World from ClientDataHandler
                    Main.ActiveWorldFileData = Main.WorldList[worldIdToLoad];

                    string text = "-autoshutdown -password \"" + Main.ConvertToSafeArgument(Netplay.ServerPassword) + "\" -lang " + Language.ActiveCulture.LegacyId;
                    if (Platform.IsLinux)
                        text = ((IntPtr.Size != 8) ? (text + " -x86") : (text + " -x64"));

                    text = ((!Main.ActiveWorldFileData.IsCloudSave) ? (text + Main.instance.SanitizePathArgument("world", Main.worldPathName)) : (text + Main.instance.SanitizePathArgument("cloudworld", Main.worldPathName)));
                    text = text + " -worldrollbackstokeep " + Main.WorldRollingBackupsCountToKeep;

                    // TML options
                    text += $@" -modpath ""{ModOrganizer.modPath}""";

                    if (Program.LaunchParameters.TryGetValue("-tmlsavedirectory", out var tmlsavedirectory))
                        text += $@" -tmlsavedirectory ""{tmlsavedirectory}""";
                    else if (Program.LaunchParameters.TryGetValue("-savedirectory", out var savedirectory))
                        text += $@" -savedirectory ""{savedirectory}""";

                    if (Main.showServerConsole)
                        text += " -showserverconsole";

                    Main.tServer = new Process();

                    Main.tServer.StartInfo.FileName = Environment.ProcessPath;
                    Main.tServer.StartInfo.Arguments = "tModLoader.dll -server " + text;
                    if (Main.libPath != "")
                    {
                        ProcessStartInfo startInfo = Main.tServer.StartInfo;
                        startInfo.Arguments = startInfo.Arguments + " -loadlib " + Main.libPath;
                    }

                    Main.tServer.StartInfo.UseShellExecute = true;
                    if (!Main.showServerConsole)
                        Main.tServer.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    if (SocialAPI.Network != null)
                        SocialAPI.Network.LaunchLocalServer(Main.tServer, Main.MenuServerMode);
                    else
                        Main.tServer.Start();
                }
                catch (Exception e)
                {
                    // log it
                    logger.Error("Failed to start server!!! C:/Program Files (x86)/Steam/steamapps/common/tModLoader/start-tModLoaderServer.bat" + e.Message);
                }

                hookForUnload?.Dispose();
                return o;
            });
        }

        /// <summary>
        /// Prepares the client data for the reload process.
        /// </summary>
        /// <param name="clientMode"></param>
        private static void PrepareClient(ClientMode clientMode)
        {
            ClientDataJsonHelper.ClientMode = clientMode;
            ClientDataJsonHelper.PlayerID = Utilities.FindPlayerId();
            ClientDataJsonHelper.WorldID = Utilities.FindWorldId();
            Log.Info("set player and worldid to " + ClientDataJsonHelper.PlayerID + " and " + ClientDataJsonHelper.WorldID);
        }

        /// <summary> A little messy function to close all other panels and open the mods panel. </summary>
        private static bool CheckIfModsToReloadIsEmpty()
        {
            if (ModsToReload.Count == 0)
            {
                MainSystem sys = ModContent.GetInstance<MainSystem>();

                // Open the mods panel.
                List<DraggablePanel> allPanels = sys?.mainState?.AllPanels;

                // replace with THIS panel
                var modSourcesPanel = sys?.mainState?.modSourcesPanel;

                // Disable all other panels
                // if (!Conf.C.AllowMultiplePanelsOpenSimultaneously)
                // {
                if (allPanels != null)
                {
                    foreach (var p in allPanels?.Except([modSourcesPanel]))
                    {
                        if (p != modSourcesPanel && p.GetActive())
                        {
                            p?.SetActive(false);
                        }
                    }
                }
                // }

                // Set the mods panel active
                modSourcesPanel?.SetActive(true);

                // Set modsbutton parentactive to true, and set the panel active
                List<BaseButton> allButtons = sys?.mainState?.AllButtons;
                var modsButton = allButtons?.FirstOrDefault(b => b is ModSourcesButton);
                if (modsButton != null)
                {
                    modsButton.ParentActive = true;
                }

                // Disable World, Log, UI, Mods buttons
                // if (!Conf.C.AllowMultiplePanelsOpenSimultaneously)
                // {
                foreach (var button in sys.mainState.AllButtons)
                {
                    if (button is ModsButton)
                    {
                        button.ParentActive = false;
                    }
                }
                // }

                return false;
            }
            return true;
        }

        /// <summary>
        /// Bruh just read the name of the function
        /// </summary>
        private static void ReloadMods()
        {
            // Going to reload mod menu(that automaticly invokes reload)
            Main.menuMode = 10002;
        }

        /// <summary>
        /// Exits the world and saves it if needed.
        /// </summary>
        /// <returns></returns>
        private static Task ExitWorld()
        {
            // We cant exit any world if in the main menu, so we just return a completed task
            if (Main.gameMenu)
            {
                return Task.CompletedTask;
            }

            if (Conf.C.SaveWorldBeforeReloading)
            {
                Log.Warn("Saving and quitting...");

                // Creating task that will delay reloading a mod until world finish saving
                var tcs = new TaskCompletionSource();
                WorldGen.SaveAndQuit(tcs.SetResult);
                return tcs.Task;
            }
            else
            {
                Log.Warn("Just quitting...");
                WorldGen.JustQuit();
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Checks if the mods to reload are in the mod sources.
        /// </summary>
        /// <returns>True, if at least one mod was found in the mod sources</returns>
        private static bool CheckThatModsExists()
        {
            Type modCompileType = typeof(Main).Assembly.GetType("Terraria.ModLoader.Core.ModCompile");
            MethodInfo findModSourcesMethod = modCompileType.GetMethod("FindModSources", BindingFlags.NonPublic | BindingFlags.Static);
            string[] modSources = (string[])findModSourcesMethod.Invoke(null, null);
            string[] modNames = modSources.Select(Path.GetFileName).ToArray();

            foreach (var modName in ModsToReload)
            {
                Log.Info("Reloading mod: " + modName);
                if (modNames.Contains(modName))
                {
                    Log.Info($"Mod '{modName}' found in mod sources.");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Builds and reloads the mods in the ModsToReload list.<br/>
        /// After building, it invokes the actionAfterBuild action if provided.
        /// </summary>
        /// <param name="actionAfterBuild">Action to invoke after building the mods.</param>
        private static void BuildAndReloadMods(Action actionAfterBuild = null)
        {
            // 1. Getting Assembly
            Assembly tModLoaderAssembly = typeof(Main).Assembly;

            // 2. Getting method for finding modSources paths
            Type modCompileType = tModLoaderAssembly.GetType("Terraria.ModLoader.Core.ModCompile");
            MethodInfo findModSourcesMethod = modCompileType.GetMethod("FindModSources", BindingFlags.NonPublic | BindingFlags.Static);
            string[] modSources = (string[])findModSourcesMethod.Invoke(null, null);

            // 3. Get all modPaths for future
            Log.Info("Executing Mods to reload: " + string.Join(", ", ModsToReload));

            var modPaths = ModsToReload.Select((modName) =>
                modSources.FirstOrDefault(p =>
                    !string.IsNullOrEmpty(p) &&
                    Directory.Exists(p) &&
                    Path.GetFileName(p)?.Equals(modName, StringComparison.InvariantCultureIgnoreCase) == true)).ToList();

            // 4. Getting method for reloading a mod
            // 4.1 Getting UIBuildMod Instance
            Type interfaceType = tModLoaderAssembly.GetType("Terraria.ModLoader.UI.Interface");
            FieldInfo buildModField = interfaceType.GetField("buildMod", BindingFlags.NonPublic | BindingFlags.Static);
            object buildModInstance = buildModField?.GetValue(null);

            // 4.2 Getting correct BuildMod method of UIBuildMod
            Type uiBuildModType = tModLoaderAssembly.GetType("Terraria.ModLoader.UI.UIBuildMod");
            MethodInfo buildModMethod = uiBuildModType.GetMethod("BuildMod", BindingFlags.Instance | BindingFlags.NonPublic, [typeof(Action<>).MakeGenericType(modCompileType), typeof(bool)]);

            // Check if it exist
            if (buildModMethod == null)
            {
                Log.Warn("No buildMethod were found via reflection.");
                return;
            }

            // 4.3 Getting correct Build method from ModCompile
            MethodInfo mcBuildModFolder = modCompileType.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance, [typeof(string)]);

            Log.Info("Starting to build mods..." + string.Join(", ", modPaths));

            // 6. Creating a task
            Main.menuMode = 10003;
            Task.Run(() =>
            {
                try
                {
                    return (Task)buildModMethod.Invoke(buildModInstance,
                    [
                        (Action<object>) (mc =>
                        {
                            for (int i = 0; i < modPaths.Count; i++)
                            {
                                string modPath = modPaths[i];
                                Log.Info("Building mod: " + modPath);
                                try
                                {
                                    mcBuildModFolder.Invoke(mc, [modPath]);
                                }
                                catch (TargetInvocationException ex)
                                {
                                    throw ex.InnerException!;
                                }
                            }
                            actionAfterBuild?.Invoke();
                        }),
                        true
                    ]);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException!;
                }

            });
        }
    }
}