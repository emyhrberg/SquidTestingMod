using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Common.Configs;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModHelper.UI.Elements
{
    public class ModFilterChangeView : UIImage
    {
        public Asset<Texture2D> tex;

        public enum ModView
        {
            Small,
            Large
        }

        public ModView currentModView = ModView.Large; // Default to large view

        public ModFilterChangeView(Asset<Texture2D> tex) : base(tex)
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

            // Toggle between small and large view
            if (currentModView == ModView.Small)
            {
                currentModView = ModView.Large;
            }
            else
            {
                currentModView = ModView.Small;
            }
            // Log.Info("switching to " + currentModView);

            // rebuild UIList
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            if (sys != null && sys.mainState != null)
            {
                // sys.OnWorldLoad();
                RebuildModLists(sys.mainState.modsPanel);
            }
        }

        private static void RebuildModLists(ModsPanel panel)
        {
            // Clear entire UIList 
            panel.uiList.Clear();

            // Add initial top container and padding
            panel.AddPadding(10f);
            panel.uiList.Add(panel.topContainer);
            panel.AddPadding(20f);

            // Clear existing lists
            panel.enabledMods.Clear();
            panel.allMods.Clear();

            panel.ConstructEnabledMods();
            panel.ConstructAllMods();

            // Ensure everything is properly displayed
            panel.Recalculate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Log.Info("Current ModView: " + currentModView);

            // Determine the source rectangle based on the view
            Rectangle sourceRect;
            if (currentModView == ModView.Small)
            {
                // Upper half (small image)
                sourceRect = new Rectangle(0, 0, 32, 32);
            }
            else
            {
                // Lower half (large image)
                sourceRect = new Rectangle(0, 32, 32, 32);
            }

            // Calculate the destination rectangle. We use GetDimensions() from the UIElement,
            // which returns a calculated position and size that respects the element's properties.
            // You may need to adjust this if you require additional scaling.
            Rectangle destinationRect = GetDimensions().ToRectangle();

            // Draw the selected portion of the texture
            spriteBatch.Draw(tex.Value, destinationRect, sourceRect, Color.White);

            // Draw tooltip if applicable
            if (IsMouseHovering && Conf.C.ShowTooltips)
            {
                // Show the current enum
                UICommon.TooltipMouseText(currentModView.ToString());
            }
        }

    }
}