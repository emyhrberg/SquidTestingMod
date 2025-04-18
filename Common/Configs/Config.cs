﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ModHelper.Common.Systems.Menus;
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

        [DrawTicks]
        [OptionStrings(["Default", "Publicizer"])]
        [DefaultValue("Default")]
        public string Compiler;

        [DefaultValue(false)]
        public bool AutoJoinWorld;

        [DefaultValue(false)]
        public bool SaveWorldBeforeReloading;

        [Header("DebugText")]

        [DefaultValue(true)]
        public bool ShowDebugText = true;

        [DefaultValue(true)]
        public bool ShowDebugActions = true;

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

        [Header("Misc")]

        [DefaultValue(false)]
        public bool AllowMultiplePanelsOpenSimultaneously = false;

        [DefaultValue(true)]
        public bool AllowDraggingPanels = true;

        [DefaultValue(false)]
        public bool ResetPanelPositionWhenToggling = false;

        [DefaultValue(true)]
        public bool ShowSearchboxBlinker = true;

        [DefaultValue(true)]
        public bool RightClickButtonToExitWorld = true;

        [DefaultValue(true)]
        public bool ShowTooltips = true;

        [DefaultValue(true)]
        public bool ShowIconsWhenHovering = true;

        [DefaultValue(true)]
        public bool AlwaysUpdateBuiltAgo = true;

        [DefaultValue(false)]
        public bool ToggleOnClickEntireElement = false;

        [DefaultValue(false)]
        public bool AutoFocusSearchBox = false;

        [DrawTicks]
        [OptionStrings(["Small", "Large"])]
        [DefaultValue("Large")]
        public string ModsView;

        // ------------------------------------------------------
        // ON CHANGED CONFIG
        // This is a risky method that sometimes causes bugs due to the way it's run kinda frequently.
        // It is called when the config is changed, and it will recreate the mainstate and reopen the previous open panel.
        // ------------------------------------------------------

        public override void OnChanged()
        {
            base.OnChanged();

            // Save panel states before any recreation
            var activePanels = SavePanelStates();
            var activeConfig = SaveOpenConfig();

            // Only recreate MainState once
            RecreateMainstate();

            // Restore panel states
            RestorePanelStates(activePanels);
            RestoreOpenConfig(activeConfig);

            UpdateExceptionState();
        }

        private string SaveOpenConfig()
        {
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null)
            {
                foreach (var panel in sys.mainState.AllPanels)
                {
                    if (panel is DraggablePanel draggablePanel && draggablePanel.GetActive())
                    {
                        // check the ModsPanel, ModsElement, ModConfigIcons
                        if (draggablePanel is ModsPanel modsPanel)
                        {
                            foreach (var mod in modsPanel.enabledMods)
                            {
                                if (mod.modConfigIcon.isConfigOpen)
                                {
                                    // Save the mod name or any identifier you want to use
                                    return mod.modConfigIcon.modName;
                                }
                            }
                        }

                    }
                }
            }
            return "";
        }

        private void RestoreOpenConfig(string nameOfModWithOpenConfig)
        {
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null)
            {
                foreach (var panel in sys.mainState.AllPanels)
                {
                    if (panel is DraggablePanel draggablePanel && draggablePanel.GetActive())
                    {
                        // check the ModsPanel, ModsElement, ModConfigIcons
                        if (draggablePanel is ModsPanel modsPanel)
                        {
                            foreach (var mod in modsPanel.enabledMods)
                            {
                                if (mod.internalModName == nameOfModWithOpenConfig)
                                {
                                    mod.modConfigIcon.SetStateToOpen();
                                }
                            }
                        }

                    }
                }
            }
        }

        private Dictionary<Type, bool> SavePanelStates()
        {
            var activePanels = new Dictionary<Type, bool>();

            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null)
            {
                foreach (var panel in sys.mainState.AllPanels)
                {
                    if (panel is DraggablePanel draggablePanel)
                    {
                        activePanels[draggablePanel.GetType()] = panel.GetActive();
                    }
                }
            }

            return activePanels;
        }

        private void RestorePanelStates(Dictionary<Type, bool> activePanels)
        {
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null && activePanels != null)
            {
                foreach (var panel in sys.mainState.AllPanels)
                {
                    if (activePanels.TryGetValue(panel.GetType(), out bool isActive))
                    {
                        panel.SetActive(isActive);
                        panel.AssociatedButton.ParentActive = isActive;
                    }
                }
            }
        }

        // Keep this simple
        private void RecreateMainstate()
        {
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null)
            {
                sys.OnWorldLoad();
            }
        }
        private static void UpdateExceptionState()
        {
            // Only try to remove the button if ImproveExceptionMenu is disabled and the hook class exists
            if (Conf.C != null && !Conf.C.ImproveExceptionMenu)
            {
                try
                {
                    ExceptionHookv2.RemoveCopyButtonFromErrorUI();
                }
                catch
                {
                    Log.Error("An error occurred while removing the button from the Exception Menu.");
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

        // [DefaultValue(true)]
        // public bool ShowOptionsButton = true;

        // [DefaultValue(true)]
        // public bool ShowUIButton = true;

        [DefaultValue(true)]
        public bool ShowModsButton = true;

        [DefaultValue(true)]
        public bool ShowModSourcesButton = true;

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
