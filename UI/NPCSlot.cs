using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SquidTestingMod.Common.Configs;
using SquidTestingMod.Helpers;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace SquidTestingMod.UI
{
    // A minimal NPCSlot that displays an NPC in a shop-style slot.
    public class NPCSlot : UIElement
    {
        private int slotContext;
        private NPC displayNPC;

        // Constructor: takes an NPC and a slot context.
        public NPCSlot(NPC npc, int slotContext)
        {
            this.slotContext = slotContext;
            displayNPC = npc;
            // Set desired clickable area (adjust as needed)
            Width.Set(50, 0f);
            Height.Set(50, 0f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetInnerDimensions();
            float bgOpacity = IsMouseHovering ? 0.9f : 0.8f;

            // Draw background inventory slot (using InventoryBack14)
            Texture2D bg = TextureAssets.InventoryBack14.Value;
            spriteBatch.Draw(bg, dimensions.ToRectangle(), Color.White * bgOpacity);

            // Draw the NPC sprite.
            // Load the NPC texture.
            Main.instance.LoadNPC(displayNPC.type);
            Asset<Texture2D> npcTextureAsset = TextureAssets.Npc[displayNPC.type];
            if (npcTextureAsset != null && npcTextureAsset.Value != null)
            {
                Texture2D npcTexture = npcTextureAsset.Value;
                int desiredSize = 48; // target size (maximum dimension)
                float scale = desiredSize / (float)Math.Max(npcTexture.Width, npcTexture.Height);
                float x = (dimensions.Width - npcTexture.Width * scale) / 2f;
                float y = (dimensions.Height - npcTexture.Height * scale) / 2f;
                Vector2 drawPos = dimensions.Position() + new Vector2(x, y);
                spriteBatch.Draw(npcTexture, drawPos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            // If hovering, display the NPC's name.
            if (IsMouseHovering)
            {
                Main.hoverItemName = displayNPC.FullName;
            }
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            // For example, simply print the NPC's name when clicked.
            Main.NewText(displayNPC.FullName);
        }
    }
}
