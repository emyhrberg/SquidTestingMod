using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Common.Configs;
using ModHelper.Common.Systems;
using ModHelper.Helpers;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ModHelper.UI.Elements
{
    public class DebugText : UIText
    {
        private bool Active = true;

        public DebugText(string text, float scale = 0.4f, bool large = true) : base(text, scale, large)
        {
            TextColor = Color.White;
            VAlign = 0.9f;
            HAlign = 0.02f;

            float w = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, Vector2.One).X;
            float h = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, Vector2.One).Y;
            Width.Set(w, 0);
            Height.Set(h * 2, 0);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            TextColor = Color.Yellow;
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            TextColor = Color.White;
        }

        public override void RightClick(UIMouseEvent evt)
        {
            Active = !Active;

            Conf.C.ShowDebugText = !Conf.C.ShowDebugText; // toggle the config value
            Conf.Save();

            if (Active)
            {
                ChatHelper.NewText("Show DebugText");
            }
            else
            {
                ChatHelper.NewText("Hide DebugText");
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // update the text to show playername, whoAmI, and FPS
            string playerName = Main.LocalPlayer.name;
            int fps = Main.frameRate;
            int ups = Main.updateRate;
            int myPlayer = Main.myPlayer; // the ID of the current player
            string text = $"{playerName} ({myPlayer})" +
                $"\n{fps}fps {ups}ups ({Main.upTimerMax.ToString("0.0")}ms)";

            SetText(text, textScale: 0.4f, large: true);
            // Log.Info("Setting text: " + text);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Active)
            {
                return;
            }

            base.Draw(spriteBatch);

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true; // disable item use if the button is hovered
                UICommon.TooltipMouseText("Right click to toggle");
            }
        }
    }

}