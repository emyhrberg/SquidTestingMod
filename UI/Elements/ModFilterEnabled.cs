using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Common.Configs;
using ModHelper.Helpers;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModHelper.UI.Elements
{
    public class ModFilterEnabled : UIImage
    {
        public Asset<Texture2D> tex;

        public enum ModFilterEnabledDisabled
        {
            All,
            Enabled,
            Disabled
        }

        public ModFilterEnabledDisabled currentEnabledDisabledView = ModFilterEnabledDisabled.All; // Default to all view

        public ModFilterEnabled(Asset<Texture2D> tex) : base(tex)
        {
            float size = 23f;
            MaxHeight.Set(size, 0f);
            MaxWidth.Set(size, 0f);
            Width.Set(size, 0f);
            Height.Set(size, 0f);

            // set texture
            this.tex = tex;
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);

            SoundEngine.PlaySound(SoundID.MenuClose);

            // Toggle between all modes using % indexing
            currentEnabledDisabledView = (ModFilterEnabledDisabled)(((int)currentEnabledDisabledView + 1) % Enum.GetValues(typeof(ModFilterEnabledDisabled)).Length);
            // Log.Info("switching to " + currentModView);

            // rebuild UIList
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null && sys.mainState.modsPanel != null)
            {
                sys.mainState.modsPanel.FilterMods();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Determine the source rectangle based on the current enabled/disabled view
            Rectangle sourceRect;

            // Map enum values to their respective positions in the texture
            // Each icon takes 32 pixels in height
            int yOffset = (int)currentEnabledDisabledView * 32;
            sourceRect = new Rectangle(0, yOffset, 32, 32);

            // Calculate the destination rectangle
            Rectangle destinationRect = GetDimensions().ToRectangle();

            // Draw the selected portion of the texture
            spriteBatch.Draw(tex.Value, destinationRect, sourceRect, Color.White);

            // Draw tooltip if applicable
            if (IsMouseHovering && Conf.C.ShowTooltips)
            {
                // Show the current enum
                UICommon.TooltipMouseText(currentEnabledDisabledView.ToString());
            }
        }

    }
}