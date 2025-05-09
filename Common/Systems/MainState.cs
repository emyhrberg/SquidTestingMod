﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Common.Configs;
using ModHelper.Helpers;
using ModHelper.UI.Elements;
using ModHelper.UI.Elements.DebugElements;
using ModHelper.UI.Elements.ModElements;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ModHelper.Common.Systems
{
    public class MainState : UIState
    {
        public ModsButton modsButton;
        public ReloadSPButton reloadSPButton;
        public ReloadMPButton reloadMPButton;

        public ModsPanel modsPanel;

        public bool Active = true;
        public bool AreButtonsShowing = false;
        public float ButtonSize = 70f;
        public float TextSize = 0.9f;
        public float offset = 0;

        public List<BaseButton> AllButtons = [];
        public List<BasePanel> AllPanels = [];

        #region Constructor
        public MainState()
        {
            offset = -ButtonSize;

            modsPanel = AddPanel<ModsPanel>();

            // Reload Buttons
            if (!AnyCheatModEnabled())
            {
                Collapse collapse = new(Ass.CollapseDown, Ass.CollapseUp);
                Append(collapse);

                AreButtonsShowing = true;

                // Buttons
                modsButton = AddButton<ModsButton>(Ass.ButtonMods, "Mods", "Manage Mods", hoverTextDescription: "Toggle mods on or off");

                // Panels
                modsButton.AssociatedPanel = modsPanel;
                modsPanel.AssociatedButton = modsButton;

                string reloadHoverMods = ReloadUtilities.IsModsToReloadEmpty ? "No mods selected" : string.Join(",", Conf.C.ModsToReload);

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    reloadSPButton = AddButton<ReloadSPButton>(Ass.ButtonReloadSP, buttonText: "Reload", hoverText: "Reload", hoverTextDescription: reloadHoverMods);
                    Append(reloadSPButton);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    reloadMPButton = AddButton<ReloadMPButton>(Ass.ButtonReloadMP, buttonText: "Reload", hoverText: "Reload", hoverTextDescription: reloadHoverMods);
                    Append(reloadMPButton);
                }
            }

            // Debug. 
            // Always add to MainState, but only show if its enabled (in Draw() and Update().. See the implementation in its class.
                DebugText debugText = new("");
                Append(debugText);

                string logFileName = Path.GetFileName(Logging.LogPath);
                DebugAction openLog = new("Open log, ", $"Open {logFileName}", Conf.C.OpenLogType == "File" ? Log.OpenClientLog : Log.OpenLogFolder);
                DebugAction clearLog = new("Clear log", $"Clear {logFileName}", Log.ClearClientLog, left: 81f);
                Append(openLog);
                Append(clearLog);
        }
        #endregion

        private bool AnyCheatModEnabled() => ModLoader.TryGetMod("CheatSheet", out _) || ModLoader.TryGetMod("HEROsMod", out _) || ModLoader.TryGetMod("DragonLens", out _);

        private T AddPanel<T>() where T : BasePanel, new()
        {
            T panel = new();
            AllPanels.Add(panel);
            Append(panel);
            return panel;
        }

        private T AddButton<T>(Asset<Texture2D> spritesheet = null, string buttonText = null, string hoverText = null, string hoverTextDescription = "") where T : BaseButton
        {
            T button = (T)Activator.CreateInstance(typeof(T), spritesheet, buttonText, hoverText, hoverTextDescription);
            button.Width.Set(ButtonSize, 0f);
            button.Height.Set(ButtonSize, 0f);
            button.Left.Set(offset, 0f);
            button.VAlign = 1.0f;
            button.HAlign = 0.5f;

            offset += ButtonSize;
            AllButtons.Add(button);
            Append(button);

            return button;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Update visibility before drawing
            foreach (var button in AllButtons)
            {
                button.Active = AreButtonsShowing;
                button.ButtonText.Active = AreButtonsShowing;
            }

            base.Draw(spriteBatch);

            if (AreButtonsShowing)
            {
                foreach (var button in AllButtons)
                {
                    if (button.IsMouseHovering && button.HoverText != null)
                    {
                        button.DrawTooltipPanel(button.HoverText, button.HoverTextDescription);
                    }
                }
            }
        }
    }
}
