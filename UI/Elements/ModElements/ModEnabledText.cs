using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModHelper.Common.Configs;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace ModHelper.UI.Elements.ModElements
{
    public class ModEnabledText : UIText
    {
        private ModState state;
        private Color red = new(226, 57, 39);
        private string internalModName;
        // private Action leftClick;

        public ModEnabledText(string text, string internalModName = "", Action leftClick = null) : base(text)
        {
            // text and size and position
            // enabledText.ShadowColor = new Color(226, 57, 39); // TODO change background color to this, shadowcolor is not it.
            float def = -65f;
            TextColor = Color.Green;
            VAlign = 0.5f;
            Left.Set(def, 1f);

            this.internalModName = internalModName;
            // this.leftClick = leftClick;
        }

        // public override void LeftClick(UIMouseEvent evt)
        // {
        //     Log.Info("Left click");

        //     base.LeftClick(evt);

        //     // toggle the state
        //     SetTextState(state == State.Enabled ? State.Disabled : State.Enabled);

        //     // set the mod state
        //     bool enabled = state == State.Enabled;

        //     MethodInfo setModEnabled = typeof(ModLoader).GetMethod("SetModEnabled", BindingFlags.NonPublic | BindingFlags.Static);
        //     setModEnabled?.Invoke(null, [internalModName, enabled]);
        // }

        public void SetTextState(ModState state)
        {
            this.state = state;
            TextColor = state == ModState.Enabled ? Color.Green : red;
            SetText(state == ModState.Enabled ? "Enabled" : "Disabled");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (IsMouseHovering)
            {
                if (state == ModState.Enabled)
                {
                    UICommon.TooltipMouseText("Click to disable");
                }
                else
                {
                    UICommon.TooltipMouseText("Click to enable");
                }
            }
        }
    }
}