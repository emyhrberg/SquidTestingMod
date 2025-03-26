using ModHelper.Helpers;
using ModHelper.PacketHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ModHelper
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class ModHelper : Mod
	{
        [Autoload(Side = ModSide.Client)]
        public class EliteTestingMod : Mod
        {
            public override void HandlePacket(BinaryReader reader, int whoAmI)
            {
                ModNetHandler.HandlePacket(reader, whoAmI);
            }

            public override void Load()
            {
                // Tweak tML's existing file appender layout at the earliest possible time

                // var hierarchy = (Hierarchy)LogManager.GetRepository();
                // var fileAppender = hierarchy.GetAppenders()
                //                             .OfType<FileAppender>()
                //                             .FirstOrDefault(a => a.Name == "FileAppender");
                // if (fileAppender != null)
                // {
                //     var layout = new PatternLayout
                //     {
                //         ConversionPattern = "[%date{yyyy-MM-dd HH:mm:ss.fff}] [%thread/%level] [%logger]: %message%newline"
                //     };
                //     layout.ActivateOptions();

                //     fileAppender.Layout = layout;
                //     fileAppender.ActivateOptions();
                // }

                Log.Info("EliteTestingMod loaded");


                if (Main.netMode != NetmodeID.Server)
                    ClientDataHandler.ReadData();
                /*
                HookEndpointManager.Clear();
                foreach (var d in DetourManager.GetDetourInfo(typeof(ModLoader).GetMethod(
                            "Unload",
                            BindingFlags.NonPublic | BindingFlags.Static
                        )).Detours.ToList())
                {
                    Log.Info($"{d} is Undo");
                    d.Undo();
                }
                var hookForUnload = new Hook(typeof(ModLoader).GetMethod(
                            "Unload",
                            BindingFlags.NonPublic | BindingFlags.Static
                        ), (Func<bool> orig) =>
                        {
                            bool o = orig();
                            LogManager.GetLogger("SQUID").Info("Hi!");
                            return o;
                        });

                //stops GC from deleting it
                GC.SuppressFinalize(hookForUnload);
                //TMLData.SaveTMLData();*/

            }

            public override void Unload()
            {
                // LOL! Close stream for disablked icons.
                // MainSystem sys = ModContent.GetInstance<MainSystem>();
                // // Get disabled mods list.

                // List<ModElement> disabledMods = sys.mainState.modsPanel.disabledMods;
                // Log.Info("unloading disabled mods count: " + disabledMods.Count);
                // foreach (ModElement mod in disabledMods)
                // {
                //     mod.modIcon.openResult?.Dispose();
                // }

                if (Main.netMode != NetmodeID.Server)
                    ClientDataHandler.WriteData();
            }
        }
    }
}
