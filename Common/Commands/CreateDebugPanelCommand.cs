using System.Linq;
using Microsoft.Xna.Framework;
using SquidTestingMod.Helpers;
using SquidTestingMod.UI;
using SquidTestingMod.UI.Elements;
using Terraria;
using Terraria.ModLoader;

namespace SquidTestingMod.Common.Commands
{
    public class CreateDebugPanelCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "add";

        public override string Usage => "/add";

        public override string Description => "Create a debug panel";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            // First verify that args are of the form /panel width height e.g /panel 100 100
            if (args.Length < 2)
            {
                Main.NewText("Usage: /add width height", Color.Red);
                return;
            }

            MainSystem sys = ModContent.GetInstance<MainSystem>();
            MainState mainState = sys.mainState;

            // Create a panel with width and height specified in the args[0] and args[1]
            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);
            CustomDebugPanel panel = new CustomDebugPanel(x, y);

            // If no 3rd and 4th argument, just spawn it at the exact center using VAlign 0.5 and halign 0.5
            if (args.Length < 4)
            {
                panel.HAlign = 0.5f;
                panel.VAlign = 0.5f;
            }
            else
            {
                // If 3rd and 4th argument are present, use them as halign and valign
                panel.HAlign = float.Parse(args[2]);
                panel.VAlign = float.Parse(args[3]);
            }
            mainState.Append(panel);
        }
    }

    public class RemoveDebugPanelCommand : ModCommand
    {
        // removes all debug panels with the type "CustomDebugPanel"

        public override CommandType Type => CommandType.Chat;

        public override string Command => "remove";

        public override string Usage => "/remove";

        public override string Description => "Remove all debug panels";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            MainSystem sys = ModContent.GetInstance<MainSystem>();
            MainState mainState = sys.mainState;

            // Remove all panels of type CustomDebugPanel
            foreach (var child in mainState.Children.ToList())
            {
                if (child is CustomDebugPanel)
                {
                    mainState.RemoveChild(child);
                }
            }
        }
    }
}