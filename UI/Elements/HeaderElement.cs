using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.UI;

namespace ModHelper.UI.Elements
{
    public class HeaderElement : PanelElement
    {
        private string hover;

        public HeaderElement(string title, string hover = "", Color color = default, float HAlign = 0.5f) : base(title)
        {
            this.hover = hover;
            IsHoverEnabled = false;

            if (color == default)
            {
                color = Color.White;
            }
            textElement.TextColor = color;
            textElement.HAlign = HAlign;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (!string.IsNullOrEmpty(hover) && IsMouseHovering)
            {
                UICommon.TooltipMouseText(hover);
            }
        }
    }
}