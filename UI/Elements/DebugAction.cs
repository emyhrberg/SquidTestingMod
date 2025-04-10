using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ModHelper.UI.Elements
{
    public class DebugAction : UIText
    {
        public DebugAction(string text, float topOffset, Action leftClick) : base(text: text, textScale: 0.9f, large: false)
        {
            TextOriginX = 0;
            TextOriginY = 0;

            TextColor = new Color(230, 230, 230);

            Width.Set(80, 0);
            Height.Set(20, 0); // 20 * 3 for 3 lines of text

            Top.Set(topOffset, 0);
            Left.Set(0, 0);

            OnLeftClick += (evt, elem) =>
            {
                leftClick?.Invoke();
            };
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            //always call base! otherwise IsMouseHovering wont work
            base.MouseOver(evt);
            TextColor = Color.Yellow;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true; // this is needed to make the mouse not interact with the game world
            }

            base.Update(gameTime);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            //always call base! otherwise IsMouseHovering wont work
            base.MouseOut(evt);
            TextColor = new Color(230, 230, 230);
        }
    }
}