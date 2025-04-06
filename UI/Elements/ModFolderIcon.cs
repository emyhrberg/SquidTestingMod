using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Helpers;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ModHelper.UI.Elements
{
    public class ModFolderIcon : UIImage
    {
        private Texture2D tex;
        private string hover;
        private string modPath;

        public ModFolderIcon(Texture2D texture, string modPath, string hover = "") : base(texture)
        {
            tex = texture;
            this.hover = hover;
            this.modPath = modPath;

            float size = 23f;
            MaxHeight.Set(size, 0f);
            MaxWidth.Set(size, 0f);
            Width.Set(size, 0f);
            Height.Set(size, 0f);
            VAlign = 0.5f;
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);

            Main.NewText("Opening mod folder: " + modPath);

            try
            {
                string folder = modPath;
                Process.Start(new ProcessStartInfo($@"{folder}") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error("Error opening log folder: " + ex.Message);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            DrawHelper.DrawProperScale(spriteBatch, this, tex, scale: 1.0f);

            if (!string.IsNullOrEmpty(hover) && IsMouseHovering)
            {
                UICommon.TooltipMouseText(hover);
            }
        }
    }
}