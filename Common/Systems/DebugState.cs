using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SquidTestingMod.UI;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace SquidTestingMod.Common.Systems
{
    public class DebugState : UIState
    {
        private List<Color> rainbowColors;

        public DebugState()
        {
            GenerateRainbowColors(20);
        }

        public void DrawHitbox(UIElement element, SpriteBatch spriteBatch)
        {
            DebugSystem uiSys = ModContent.GetInstance<DebugSystem>();
            if (!uiSys.isUIDebugDrawing) return;

            if (element is DebugState || element is MainState) return; // Skip full-screen elements

            Rectangle hitbox = element.GetOuterDimensions().ToRectangle();

            // Get a color from the rainbow
            int colorIndex = element.UniqueId % rainbowColors.Count;
            Color hitboxColor = rainbowColors[colorIndex] * 0.1f;

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, hitboxColor);
            DrawOutline(spriteBatch, hitbox);
        }

        public void DrawElementLabel(SpriteBatch spriteBatch, UIElement element, Point position)
        {
            // round width and height to whole numbers
            int width = (int)element.GetOuterDimensions().Width;
            int height = (int)element.GetOuterDimensions().Height;

            string sizeText = $"{width}x{height}";

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(sizeText) * 0.7f; // Smaller text
            Vector2 textPosition = new(position.X - 5, position.Y - textSize.Y - 2); // Offset to the left by 5px

            ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch,
                FontAssets.MouseText.Value,
                sizeText,
                textPosition,
                Color.White,
                0f,
                Vector2.Zero,
                new Vector2(0.6f)); // Scale down text
        }

        private void DrawOutline(SpriteBatch spriteBatch, Rectangle hitbox)
        {
            hitbox.Inflate(1, 1);
            Texture2D t = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(t, new Rectangle(hitbox.X, hitbox.Y, hitbox.Width, 2), Color.White);
            spriteBatch.Draw(t, new Rectangle(hitbox.X, hitbox.Y, 2, hitbox.Height), Color.White);
            spriteBatch.Draw(t, new Rectangle(hitbox.X + hitbox.Width - 2, hitbox.Y, 2, hitbox.Height), Color.White);
            spriteBatch.Draw(t, new Rectangle(hitbox.X, hitbox.Y + hitbox.Height - 2, hitbox.Width, 2), Color.White);
        }

        private void GenerateRainbowColors(int count)
        {
            rainbowColors = [];
            for (int i = 0; i < count; i++)
            {
                float hue = (float)i / count;
                rainbowColors.Add(Main.hslToRgb(hue, 1f, 0.5f));
            }
        }
    }
}
