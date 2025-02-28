using Microsoft.Xna.Framework;
using SquidTestingMod.Common.Configs;
using SquidTestingMod.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SquidTestingMod.Common.Systems
{
    /// <summary>
    /// Gives player abilities for:
    /// GodMode - Makes the player invincible
    /// FastMode - Increases player speed
    /// BuildMode - Infinite range, instant mining and more
    /// NoClip - Move through blocks
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class PlayerCheats : ModPlayer
    {
        // Variables
        public static bool IsGodModeOn = false;
        public static bool IsFastModeOn = false;
        public static bool IsBuildModeOn = false;
        public static bool IsNoClipOn = false;
        public static bool IsLightModeOn = false;
        public static bool IsTeleportModeOn = false;

        #region Toggle values
        public static void ToggleGodMode()
        {
            IsGodModeOn = !IsGodModeOn;

            if (Conf.ShowCombatTextOnToggle)
                CombatText.NewText(Main.LocalPlayer.getRect(), IsGodModeOn ? Color.Green : Color.Red, IsGodModeOn ? "God Mode On" : "God Mode Off");
        }

        public static void ToggleFastMode()
        {
            IsFastModeOn = !IsFastModeOn;

            if (Conf.ShowCombatTextOnToggle)
                CombatText.NewText(Main.LocalPlayer.getRect(), IsFastModeOn ? Color.Green : Color.Red, IsFastModeOn ? "Fast Mode On" : "Fast Mode Off");

            // Show CloudInABottleJump effect when toggling fast mode
            if (IsFastModeOn)
            {
                CloudInABottleJump cloudJump = new CloudInABottleJump();
                bool playSound = true;
                cloudJump.OnStarted(Main.LocalPlayer, ref playSound);
                cloudJump.ShowVisuals(Main.LocalPlayer);
            }
        }

        public static void ToggleBuildMode()
        {
            IsBuildModeOn = !IsBuildModeOn;

            if (Conf.ShowCombatTextOnToggle)
                CombatText.NewText(Main.LocalPlayer.getRect(), IsBuildModeOn ? Color.Green : Color.Red, IsBuildModeOn ? "Build Mode On" : "Build Mode Off");
        }

        public static void ToggleNoClip()
        {
            IsNoClipOn = !IsNoClipOn;

            if (Conf.ShowCombatTextOnToggle)
                CombatText.NewText(Main.LocalPlayer.getRect(), IsNoClipOn ? Color.Green : Color.Red, IsNoClipOn ? "NoClip On" : "NoClip Off");
        }

        public static void ToggleLightMode()
        {
            IsLightModeOn = !IsLightModeOn;

            if (Conf.ShowCombatTextOnToggle)
                CombatText.NewText(Main.LocalPlayer.getRect(), IsLightModeOn ? Color.Green : Color.Red, IsLightModeOn ? "Light Mode On" : "Light Mode Off");
        }

        public static void ToggleTeleportMode()
        {
            IsTeleportModeOn = !IsTeleportModeOn;

            if (Conf.ShowCombatTextOnToggle)
                CombatText.NewText(Main.LocalPlayer.getRect(), IsTeleportModeOn ? Color.Green : Color.Red, IsTeleportModeOn ? "Teleport Mode On" : "Teleport Mode Off");
        }

        #endregion

        public override void OnEnterWorld()
        {
            if (Main.dedServ)
                return;

            // IsGodModeOn = C.StartInGodMode;
        }

        #region GodMode
        public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
        {
            if (IsGodModeOn)
                return true;
            return false;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (IsGodModeOn)
            {
                // Don't kill the player
                Player.statLife = Player.statLifeMax2;
                return false;
            }
            return true;
        }
        #endregion

        #region All cheats in postupdate
        public override void PostUpdate()
        {
            if (IsGodModeOn)
                Player.statLife = Player.statLifeMax2; // Keep player at max health

            if (IsFastModeOn)
            {
                // Increase player speed and acceleration
                // default is:
                // maxRunSpeed = 3f, runAcceleration = 0.08f, runSlowdown = 0.2f, 
                // moveSpeed = 1f where e.g += 0.25f is swiftness potion
                // Log.Info($"Fast: maxRunSpeed: {Player.maxRunSpeed}, runAcceleration: {Player.runAcceleration}, moveSpeed: {Player.moveSpeed}");

                Player.moveSpeed += 2.25f;
                Player.maxRunSpeed = 50f; // 50f is very fast, 15-20f is kinda fast
                Player.runAcceleration *= 3.5f;
                Player.runSlowdown *= 0.5f;

                // Instant acceleration for running.
                // Set this to around 5f for fast or 10f for instant (10f is too fast imo)
                if (Player.controlLeft)
                    Player.velocity.X = -Player.maxRunSpeed * 0.33f;
                else if (Player.controlRight)
                    Player.velocity.X = Player.maxRunSpeed * 0.33f;

                // When holding jump, make the player fly
                if (Player.controlJump)
                    Player.velocity.Y -= 0.5f;
            }

            if (IsNoClipOn)
            {
                Vector2 desiredPos = Player.Center;

                // Handle movement input
                if (Player.controlLeft)
                    desiredPos.X -= 15;
                if (Player.controlRight)
                    desiredPos.X += 15;
                if (Player.controlUp)
                    desiredPos.Y -= 15;
                if (Player.controlDown)
                    desiredPos.Y += 15;

                // Apply position and disable velocity
                Player.Center = desiredPos;
                Player.velocity = Vector2.Zero;
                Player.gfxOffY = 0;
            }

            if (IsTeleportModeOn && Main.mouseRight)
            {
                Main.LocalPlayer.Teleport(Main.MouseWorld);
            }
        }
        #endregion

        #region Build mode
        public override void UpdateEquips()
        {
            if (IsBuildModeOn)
            {
                // Disable smart cursor
                if (Main.SmartCursorWanted)
                {
                    Main.SmartCursorWanted_Mouse = false;
                    Main.SmartCursorWanted_GamePad = false;
                    Main.NewText("Smart Cursor Disabled in Build Mode");
                }

                // Set infinite range
                Player.tileRangeX = int.MaxValue;
                Player.tileRangeY = int.MaxValue;
                Player.blockRange = int.MaxValue; // Ensures unrestricted tile placement
            }
        }

        public override float UseTimeMultiplier(Item item)
        {
            // Fast speed for tools
            if (IsBuildModeOn && (item.createTile != -1 || item.createWall != -1 || item.pick > 0 || item.axe > 0 || item.hammer > 0))
                return 0.01f; // Near-instant speed

            // Default speed
            return 1;
        }

        public override float UseAnimationMultiplier(Item item)
        {
            if (!IsBuildModeOn)
                return 1;

            // If the item is a tile, wall, or tool, make it near-instant
            if (item.createTile != -1 || item.createWall != -1 || item.pick > 0 || item.axe > 0 || item.hammer > 0)
                return 0.01f; // Near-instant speed

            return 1;
        }
        #endregion
    }
}