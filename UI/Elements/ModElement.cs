using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Common.Configs;
using ModHelper.Helpers;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using static ModHelper.UI.Elements.OptionElement;

namespace ModHelper.UI.Elements
{
    // Contains:
    // Icon image
    // Mod name
    // Enabled text
    public class ModElement : UIPanel
    {
        public string cleanModName;
        public string internalName;
        private ModEnabledText enabledText;
        public ModEnabledIcon modIcon;
        private State state = State.Enabled; // enabled by default
        private Texture2D icon;

        public ModConfigIcon modConfigIcon;
        public ModInfoIcon modInfoIcon;

        private string modDescription;

        // Actions
        // private Action leftClick;
        // private Action rightClick;

        // State
        public State GetState() => state;

        public void SetState(State state)
        {
            this.state = state;
            enabledText.SetTextState(state);
        }

        // Constructor
        public ModElement(string cleanModName, string internalModName = "", Texture2D icon = null, Action leftClick = null, Action rightClick = null, string modDescription = "")
        {
            this.cleanModName = cleanModName;
            this.internalName = internalModName;
            this.icon = icon;
            this.modDescription = modDescription;

            // this.leftClick = leftClick;
            // this.rightClick = rightClick;

            // size and position
            Width.Set(-35f, 1f);
            Height.Set(30, 0);
            Left.Set(5, 0);

            // mod icon

            // passing a temp icon because above doesnt work
            // maybe because path its not loaded yet.
            Texture2D temp = TextureAssets.MagicPixel.Value;

            modIcon = new(temp, internalModName, icon: icon);
            Append(modIcon);

            // mod name
            if (cleanModName.Length > 20)
                cleanModName = string.Concat(cleanModName.AsSpan(0, 20), "...");

            // if icon is not null, it means the mod is not loaded.
            // this is because we send the icon in the constructor for unloaded mods.
            // so we should not allow the user to click on it.
            // so we send no hover option
            float size = 25f;
            if (icon == null)
            {
                // "Enabled Mods"
                ModTitleText modNameText = new(text: cleanModName, hover: internalModName, internalModName: internalModName);
                modNameText.Left.Set(95, 0); // 25 left of icon1, 25 left of config, 5+5 padding=60+5 padding
                modNameText.VAlign = 0.5f;
                Append(modNameText);

                // Add ModConfigIcon to enabled mods IF they have a config.
                // if (ModLoader.GetMod(internalModName).GetConfig(internalModName) != null)
                // {
                modConfigIcon = new(texture: Ass.ConfigOpen, modPath: internalName, hover: $"Open {cleanModName} config", cleanModName: cleanModName);
                modConfigIcon.VAlign = 0.5f;
                modConfigIcon.MaxHeight.Set(size, 0f);
                modConfigIcon.MaxWidth.Set(size, 0f);
                modConfigIcon.Width.Set(size, 0f);
                modConfigIcon.Height.Set(size, 0f);
                modConfigIcon.Top.Set(-1, 0); // custom top
                modConfigIcon.Left.Set(60, 0); // 25 to left of icon + 5 padding
                Append(modConfigIcon);
                // }
            }
            else
            {
                // "All Mods"
                ModTitleText modNameText = new(text: cleanModName, internalModName: internalModName, hover: $"{internalModName}");
                modNameText.Left.Set(60, 0);
                modNameText.VAlign = 0.5f;
                Append(modNameText);


            }

            // Mod InfoIcon
            modInfoIcon = new(texture: Ass.ModInfo, modPath: internalModName, hover: $"Open {cleanModName} info", modDescription: modDescription, modCleanName: cleanModName);
            modInfoIcon.VAlign = 0.5f;
            modInfoIcon.MaxHeight.Set(size, 0f);
            modInfoIcon.MaxWidth.Set(size, 0f);
            modInfoIcon.Width.Set(size, 0f);
            modInfoIcon.Height.Set(size, 0f);
            modInfoIcon.Top.Set(-1, 0); // custom top
            modInfoIcon.Left.Set(30, 0); // 25 to left of icon + 5 padding
            Append(modInfoIcon);

            // enabled text.
            // if no icon, its an enabled mod, so we manually add the left click action.
            enabledText = new ModEnabledText(text: "Enabled", internalModName: internalModName, leftClick: icon == null ? leftClick : null);
            Append(enabledText);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            // check if we also clicked the config, then we shouldnt execute the left click action.
            // this was wonky and worked but was laggy ??? evt.Target sucks ???
            // if (icon == null && evt.Target is ModTitleText modTitleText && modTitleText.clickToOpenConfig)
            // {
            //     return;
            // }

            // First, check if the click target is the ModConfigIcon or any of its children
            if (evt.Target is ModConfigIcon modConfigIcon)
            {
                // If clicked on config icon, don't proceed with mod toggling
                return;
            }

            if (!Conf.C.ToggleOnClickEntireElement && evt.Target is not ModEnabledText modEnabledText)
            {
                // If clicked on enabled text, don't proceed with mod toggling
                return;
            }

            base.LeftClick(evt);

            // Log.Info("LeftClick on text: " + internalName);

            // Update enabled text state first
            SetState(state == State.Enabled ? State.Disabled : State.Enabled);
            enabledText.SetTextState(state);

            // Use reflection to call SetModEnabled on internalModName
            bool enabled = state == State.Enabled;

            MethodInfo setModEnabled = typeof(ModLoader).GetMethod("SetModEnabled", BindingFlags.NonPublic | BindingFlags.Static);
            setModEnabled?.Invoke(null, [internalName, enabled]);
        }
    }
}