﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using ModHelper.Helpers;
using ModHelper.UI;
using ModHelper.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ModHelper.Common.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Reload")]

        [DefaultValue("")]
        public string ModToReload;

        [DefaultValue(true)]
        public bool AutoJoinWorldAfterUnload;

        [DefaultValue(false)]
        public bool SaveWorldBeforeReloading;

        [Header("Debug Text")]

        [DrawTicks]
        [OptionStrings(["Off", "Small", "Medium", "Large"])]
        [DefaultValue("Medium")]
        public string SizeDebugText;

        [Header("Improvements")]
        [DefaultValue(true)]
        public bool ImproveMainMenu;

        [DefaultValue(true)]
        public bool ImproveExceptionMenu;

        // show by default, since its easier to modify ingame for the user like this.
        // [Expand(false, false)]
        [Header("Buttons")]

        [DrawTicks]
        [OptionStrings(["Left", "Right"])]
        [DefaultValue("Right")]
        public string DragButtons;

        public Buttons Buttons = new();

        public override void OnChanged()
        {
            base.OnChanged();

            // remove and re-create entire MainState.
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null)
            {
                // Check active panels to re-create them
                var activePanels = new Dictionary<Type, bool>();
                foreach (var panel in sys.mainState.AllPanels)
                {
                    if (panel is DraggablePanel draggablePanel)
                    {
                        activePanels[draggablePanel.GetType()] = panel.GetActive();
                    }
                }

                // Log all active panels in one line
                Log.Info("Active panels: " + string.Join(", ", activePanels.Keys));

                // Create a new MainState (reset everything)
                sys.OnWorldLoad();

                // Restore active panels
                foreach (var panel in sys.mainState.AllPanels)
                {
                    if (activePanels.TryGetValue(panel.GetType(), out bool isActive))
                    {
                        panel.SetActive(isActive);
                    }
                }
            }
        }
    }

    public class Buttons
    {
        // [Range(50f, 80f)]
        // [Increment(5f)]
        // [DefaultValue(70)]
        // public float ButtonSize;

        [DefaultValue(true)]
        public bool ShowOptionsButton = true;

        [DefaultValue(true)]
        public bool ShowUIButton = true;

        [DefaultValue(true)]
        public bool ShowModsButton = true;

        [DefaultValue(true)]
        public bool ShowReloadButton = true;

        [DefaultValue(false)]
        public bool ShowReloadMPButton = false;
    }

    public static class Conf
    {
        /// <summary>
        /// There is no current way to manually save a mod configuration file in tModLoader.
        // / The method which saves mod config files is private in ConfigManager, so reflection is used to invoke it.
        /// CalamityMod does this great
        /// Reference: 
        // https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/CalamityMod.cs#L550
        /// </summary>
        public static void Save()
        {
            try
            {
                MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
                if (saveMethodInfo is not null)
                    saveMethodInfo.Invoke(null, [Conf.C]);
            }
            catch
            {
                Log.Error("An error occurred while manually saving ModConfig!.");
            }
        }

        // Instance of the Config class
        // Use it like Conf.C for easy access to the config values
        public static Config C => ModContent.GetInstance<Config>();
        public static Buttons Show => C.Buttons;
    }
}
