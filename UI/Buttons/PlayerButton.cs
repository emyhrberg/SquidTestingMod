using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Common.Players;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ModHelper.UI.Buttons
{
    public class PlayerButton(Asset<Texture2D> spritesheet, string buttonText, string hoverText, string hoverTextDescription) : BaseButton(spritesheet, buttonText, hoverText, hoverTextDescription)
    {
        // Set custom animation dimensions
        private float _scale = 0.9f;
        protected override float Scale => _scale;
        protected override int StartFrame => 3;
        protected override int FrameCount => 17;
        protected override int FrameSpeed => 5;
        protected override int FrameWidth => 44;
        protected override int FrameHeight => 54;

        public override void LeftClick(UIMouseEvent evt)
        {
            // Toggle player panel
            var sys = ModContent.GetInstance<MainSystem>();
            var allPanels = sys?.mainState?.RightSidePanels;
            var playerPanel = sys?.mainState?.playerPanel;

            // Close other panels
            foreach (var panel in allPanels.Except([playerPanel]))
            {
                if (panel.GetActive())
                {
                    panel.SetActive(false);
                }
            }

            // Toggle player panel
            if (playerPanel.GetActive())
                playerPanel.SetActive(false);
            else
                playerPanel.SetActive(true);
        }

        public override void RightClick(UIMouseEvent evt)
        {
            //base.RightClick(evt);

            // Toggle super mode
            PlayerCheatManager p = Main.LocalPlayer.GetModPlayer<PlayerCheatManager>();

            p.SuperMode = !p.SuperMode;
            if (p.SuperMode)
                p.EnableSupermode();
            else
                p.DisableSupermode();
        }
    }
}
